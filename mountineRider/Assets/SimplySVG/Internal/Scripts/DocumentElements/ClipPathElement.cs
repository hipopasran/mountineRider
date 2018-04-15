using UnityEngine;
using System.Collections.Generic;
using ClipperLib;
using System;

namespace SimplySVG {
    public class ClipPathElement : SVGElement, SVGStylable, SVGTransformable {
        GraphicalAttributes localGraphicalAttributes;
        TransformAttributes localTransformAttributes;

        protected PolyTree stencil;

        public ClipPathElement() {
            localGraphicalAttributes = new GraphicalAttributes();
            localTransformAttributes = new TransformAttributes();
        }

        delegate void RecurseSVGElements(
            SVGElement node,
            CascadeContext parentCascadeContext
        );
        public PolyTree GetStencil(
            CascadeContext userCascadeContext
        ) {
            Clipper stencilClipper = new Clipper();
            stencilClipper.StrictlySimple = true;

            // Add all child shapes to the clipping operation...
            RecurseSVGElements recurse = null;
            recurse = delegate (
                SVGElement node,
                CascadeContext parentCascadeContext
            ) {
                CascadeContext cascadeContext = parentCascadeContext.GatherElement(node);

                if (node is GraphicalElement) {
                    GraphicalElement shapeElement = (GraphicalElement)node;

                    // Build and clip the shape contour
                    if (!shapeElement.BuildStencil(
                        cascadeContext,
                        ownerDocument.importSettings
                    )) {
                        throw new System.Exception("Building shape stencil failed");
                    }

                    // Gather the shape contour
                    if (shapeElement.stencilTree != null) {
                        stencilClipper.AddPaths(
                            Clipper.PolyTreeToPaths(shapeElement.stencilTree),
                            PolyType.ptSubject,
                            true
                        );
                    }
                }
                
                if (node is UseElement) {
                    // The Use element does not have children, but it has the surrogate
                    // pointer that needs to be recursed.

                    UseElement useElement = (UseElement)node;
                    recurse(
                        useElement.surrogateForElement,
                        cascadeContext
                    );

                } else {
                    // This is a regular SVG element. Recurse to its children

                    for (int i = 0; i < node.children.Count; i++) {
                        recurse(
                            node.children[i],
                            cascadeContext
                        );
                    }
                }
            };

            recurse(
                this,
                userCascadeContext
            );

            stencil = new PolyTree();
            if (!stencilClipper.Execute(ClipType.ctUnion, stencil)) {
                stencil = null;
            }

            return stencil;
        }

        public override bool Triangulate(CascadeContext parentCascadeContext, ImportSettings options, ref List<Vector3> meshVertices, ref List<int> meshTriangles, ref List<Color> meshVertexColors) {
            return true;
        }

        public override bool AddAttribute(string attributeName, string attributeValue) {
            return
                base.AddAttribute(attributeName, attributeValue) ||
                AddStyleAttribute(attributeName, attributeValue) ||
                AddTransformAttribute(attributeName, attributeValue);
        }

        public bool AddStyleAttribute(string attributeName, string attributeValue) {
            return localGraphicalAttributes.AddAttribute(attributeName, attributeValue);
        }

        public bool AddTransformAttribute(string attributeName, string attributeValue) {
            return localTransformAttributes.AddAttribute(attributeName, attributeValue);
        }

        public GraphicalAttributes GetLocalAttributes() {
            return localGraphicalAttributes;
        }

        public TransformAttributes GetLocalTransformation() {
            return localTransformAttributes;
        }
    }
}
