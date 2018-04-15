using UnityEngine;
using System.Collections.Generic;
using ClipperLib;
using Poly2Tri;
using System;

namespace SimplySVG {
    [System.Serializable]
    public abstract class GraphicalElement : SVGElement, SVGStylable, SVGTransformable {
        public static readonly double clipperCoordinateScale = 8192.0;

        protected GraphicalAttributes localGraphicalAttributes;
        protected TransformAttributes localTransformAttributes;

        List<ContourPath> contourPaths;

        public PolyTree stencilTree;
        public PolyTree fillTree;
        public PolyTree openStrokeTree;
        public PolyTree closedStrokeTree;

        public GraphicalElement() {
            localGraphicalAttributes = new GraphicalAttributes();
            localTransformAttributes = new TransformAttributes();
        }

        public override bool AddAttribute(string attributeName, string attributeValue) {
            return
                base.AddAttribute(attributeName, attributeValue) ||
                AddShapeAttribute(attributeName, attributeValue) ||
                AddStyleAttribute(attributeName, attributeValue) ||
                AddTransformAttribute(attributeName, attributeValue);
        }

        public bool AddStyleAttribute(string attributeName, string attributeValue) {
            return localGraphicalAttributes.AddAttribute(attributeName, attributeValue);
        }

        public bool AddTransformAttribute(string attributeName, string attributeValue) {
            return localTransformAttributes.AddAttribute(attributeName, attributeValue);
        }

        public abstract bool AddShapeAttribute(string attributeName, string attributeValue);

        /// <summary>
        /// Build the shape type specific countour of the graphic.
        /// </summary>
        /// <returns>Build succeeded with no errors</returns>
        protected abstract List<ContourPath> BuildShape(ImportSettings options);

        protected bool BuildCountour(ImportSettings options) {
            contourPaths = BuildShape(options);

            if (contourPaths == null) {
                return false;
            }

            // Convert all paths to Clipper format
            for (int i = 0; i < contourPaths.Count; i++) {
                contourPaths[i].PopulateClipperPath();
            }

            return true;
        }

        public bool BuildStencil(
            CascadeContext cascadeContext,
            ImportSettings options
        ) {
            if (!BuildCountour(options)) {
                return false;
            }

            // Clip fill
            bool clipSuccess = TriangulationUtility.ClipFill(
                contourPaths,
                cascadeContext.graphicalAttributes.clipRule.Value,
                out stencilTree
            );

            if (!clipSuccess) {
                return false;
            }

            // Apply transformation
            ApplyTransformation(stencilTree, cascadeContext.transformAttributes.combinedTransform);

            return true;
        }

        /// <summary>
        /// Build the outline shape of the graphical element and then generate the fill and stroke shapes for triangularization.
        /// </summary>
        /// <param name="cascadeContext"></param>
        /// <param name="options"></param>
        /// <param name="forceFillOnly">If true, build, clip and transform the fill only. Use for stencil generation.</param>
        /// <returns>Build and clip were successful</returns>
        public bool ClipShape(
            CascadeContext cascadeContext,
            ImportSettings options
        ) {
            if (!BuildCountour(options)) {
                return false;
            }

            // Clip shapes
            if (cascadeContext.graphicalAttributes.useFill != null &&
                cascadeContext.graphicalAttributes.useFill.Value
            ) {
                // Clip fill
                bool fillClipSuccess = TriangulationUtility.ClipFill(
                    contourPaths,
                    cascadeContext.graphicalAttributes.fillRule.Value,
                    out fillTree
                );

                if (!fillClipSuccess) {
                    return false;
                }
            }

            // Clip stroke
            if (cascadeContext.graphicalAttributes.useStroke != null &&
                cascadeContext.graphicalAttributes.useStroke.Value
            ) {
                bool strokeClipSuccess = TriangulationUtility.ClipStroke(
                    contourPaths,
                    cascadeContext.graphicalAttributes.fillRule.Value,
                    cascadeContext.graphicalAttributes.strokeWidth.Value,
                    cascadeContext.graphicalAttributes.strokeMiterLimit.Value,
                    out openStrokeTree,
                    out closedStrokeTree
                );

                if (!strokeClipSuccess) {
                    return false;
                }
            }

            // Apply transformation
            ApplyTransformation(fillTree, cascadeContext.transformAttributes.combinedTransform);
            ApplyTransformation(closedStrokeTree, cascadeContext.transformAttributes.combinedTransform);
            ApplyTransformation(openStrokeTree, cascadeContext.transformAttributes.combinedTransform);

            return true;
        }

        public override bool Triangulate(
            CascadeContext parentCascadeContext,
            ImportSettings options,
            ref List<Vector3> meshVertices,
            ref List<int> meshTriangles,
            ref List<Color> meshVertexColors
        ) {
            CascadeContext cascadeContext = parentCascadeContext.GatherElement(this);

            if (!ClipShape(cascadeContext, options)) {
                if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_AND_WARNINGS) {
                    Debug.LogError("Shape (" + (string.IsNullOrEmpty(id) ? "no ID set" : "ID: " + id) + ") contour building failed");
                }

                return false;
            }
            
            // Apply a stencil clip if the clip-path attribute has been set
            if (cascadeContext.clipStencil != null) {
                if (fillTree != null) {
                    TriangulationUtility.ClipStencil(fillTree, cascadeContext.clipStencil, out fillTree);
                }

                if (closedStrokeTree != null) {
                    TriangulationUtility.ClipStencil(closedStrokeTree, cascadeContext.clipStencil, out closedStrokeTree);
                }

                if (openStrokeTree != null) {
                    TriangulationUtility.ClipStencil(openStrokeTree, cascadeContext.clipStencil, out openStrokeTree);
                }
            }

            // Prepare color values
            Color fillColor = cascadeContext.graphicalAttributes.fillColor.Value;
            fillColor.a =
                cascadeContext.graphicalAttributes.fillOpacity.Value *
                cascadeContext.graphicalAttributes.opacity.Value;

            Color strokeColor = cascadeContext.graphicalAttributes.strokeColor.Value;
            strokeColor.a = 
                cascadeContext.graphicalAttributes.strokeOpacity.Value *
                cascadeContext.graphicalAttributes.opacity.Value;

            // Triangulate and add to output lists
            if (fillTree != null && fillTree.Total > 0) {
                bool fillTriangulatedSuccess = TriangulationUtility.TriangulatePolyTree(
                    fillTree,
                    fillColor,
                    ref meshVertices,
                    ref meshTriangles,
                    ref meshVertexColors
                );

                if (!fillTriangulatedSuccess) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_AND_WARNINGS) {
                        Debug.LogError("Triangulating shape (" + (string.IsNullOrEmpty(id) ? "no ID set" : "ID: " + id) + ") fill failed");
                    }
                    return false;
                }
            }

            if (closedStrokeTree != null && closedStrokeTree.Total > 0) {
                bool closedStrokeTriangulatedSuccess = TriangulationUtility.TriangulatePolyTree(
                    closedStrokeTree,
                    strokeColor,
                    ref meshVertices,
                    ref meshTriangles,
                    ref meshVertexColors
                );

                if (!closedStrokeTriangulatedSuccess) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_AND_WARNINGS) {
                        Debug.LogError("Triangulating shape (" + (string.IsNullOrEmpty(id) ? "no ID set" : "ID: " + id) + ") closed stroke failed");
                    }
                    return false;
                }
            }

            if (openStrokeTree != null && openStrokeTree.Total > 0) {
                bool openStrokeTriangulatedSuccess = TriangulationUtility.TriangulatePolyTree(
                    openStrokeTree,
                    strokeColor,
                    ref meshVertices,
                    ref meshTriangles,
                    ref meshVertexColors
                );

                if (!openStrokeTriangulatedSuccess) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_AND_WARNINGS) {
                        Debug.LogError("Triangulating shape (" + (string.IsNullOrEmpty(id) ? "no ID set" : "ID: " + id) + ") open stroke failed");
                    }
                    return false;
                }
            }

            return true;
        }

        public GraphicalAttributes GetLocalAttributes() {
            return localGraphicalAttributes;
        }

        public TransformAttributes GetLocalTransformation() {
            return localTransformAttributes;
        }

        delegate void PolyTreeRecurse(PolyNode node);
        void ApplyTransformation(PolyTree shapes, Matrix transformation) {
            if (shapes == null || shapes.Total == 0 || shapes.ChildCount == 0) {
                return;
            }

            PolyTreeRecurse recurse = null;
            recurse = delegate (PolyNode node) {
                List<IntPoint> contour = node.Contour;
                for (int i = 0; i < contour.Count; i++) {
                    contour[i] = MatrixUtils.MultiplyScaledClipperPoint(transformation, contour[i]);
                }

                List<PolyNode> children = node.Childs;
                for (int i = 0; i < children.Count; i++) {
                    recurse(children[i]);
                }
            };

            recurse(shapes);
        }

        [System.Serializable]
        public class ContourPath {
            public bool closed;
            public List<Vector2> path;
            public List<IntPoint> clipperPath;

            public ContourPath(bool closed = false, List<Vector2> path = null) {
                this.closed = closed;

                if (path == null) {
                    this.path = new List<Vector2>();
                } else {
                    this.path = path;
                }
            }

            public void PopulateClipperPath() {
                clipperPath = new List<IntPoint>(path.Count);

                for (int i = 0; i < path.Count; i++) {
                    clipperPath.Add(
                        ImportUtilities.ConvertToScaledClipperPoint(
                            path[i]
                        )
                    );
                }
            }
        }
    }
}
