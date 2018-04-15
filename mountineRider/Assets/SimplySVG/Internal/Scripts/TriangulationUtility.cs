using UnityEngine;
using System.Collections.Generic;
using ClipperLib;
using Poly2Tri;

namespace SimplySVG {
    public static class TriangulationUtility {
        public static bool ClipFill(
            List<GraphicalElement.ContourPath> contourPaths,
            PolyFillType fillRule,
            out PolyTree fillTree
        ) {
            Clipper fillClipper = new Clipper();
            fillClipper.StrictlySimple = true;

            fillTree = new PolyTree();

            int pathsIncluded = 0;
            for (int i = 0; i < contourPaths.Count; i++) {
                if (contourPaths[i].clipperPath.Count < 3) {
                    continue;
                }

                fillClipper.AddPath(
                    contourPaths[i].clipperPath, PolyType.ptSubject, true
                );

                pathsIncluded++;
            }

            if (pathsIncluded < 1) {
                // Theres nothing to clip
                return true;
            }

            bool clipSuccessful = fillClipper.Execute(
                ClipType.ctUnion,
                fillTree,
                fillRule,
                fillRule
            );
            
            if (!clipSuccessful) {
                return false;
            }

            return true;
        }

        public static bool ClipStroke(
            List<GraphicalElement.ContourPath> contourPaths,
            PolyFillType fillRule,
            float strokeWidth,
            float miterLimit,
            out PolyTree openStrokeTree,
            out PolyTree closedStrokeTree
        ) {
            openStrokeTree = new PolyTree();
            closedStrokeTree = new PolyTree();

            if (strokeWidth == 0f) {
                return false;
            }
            
            bool hasOpenPaths = false;
            bool hasClosedPaths = false;

            ClipperOffset openStrokeOffsetter = new ClipperOffset(miterLimit);
            Clipper closedShapeClipper = new Clipper();
            closedShapeClipper.StrictlySimple = true;

            int pathsIncluded = 0;
            for (int i = 0; i < contourPaths.Count; i++) {
                if (contourPaths[i].closed && contourPaths[i].clipperPath.Count >= 3) { // Closed polygon
                    closedShapeClipper.AddPath(
                        contourPaths[i].clipperPath,
                        PolyType.ptSubject,
                        true
                    );

                    pathsIncluded++;
                    hasClosedPaths = true;

                } else if (contourPaths[i].clipperPath.Count >= 2) { // Open path
                    openStrokeOffsetter.AddPath(
                        contourPaths[i].clipperPath,
                        JoinType.jtMiter,
                        EndType.etOpenButt
                    );

                    pathsIncluded++;
                    hasOpenPaths = true;
                }
            }

            if (hasOpenPaths && pathsIncluded > 0) {
                // Build stroke for open shapes
                openStrokeOffsetter.Execute(
                    ref openStrokeTree,
                    (strokeWidth / 2f) * GraphicalElement.clipperCoordinateScale
                );
            }

            if (hasClosedPaths && pathsIncluded > 0) {
                // Build stroke for closed shapes
                List<List<IntPoint>> clippedClosedShape = new List<List<IntPoint>>();
                closedShapeClipper.Execute(ClipType.ctUnion, clippedClosedShape);

                ClipperOffset closedStrokeOffsetter = new ClipperOffset(miterLimit);
                closedStrokeOffsetter.AddPaths(clippedClosedShape, JoinType.jtMiter, EndType.etClosedPolygon);

                List<List<IntPoint>> closedInterior = new List<List<IntPoint>>();
                List<List<IntPoint>> closedExterior = new List<List<IntPoint>>();

                closedStrokeOffsetter.Execute(ref closedInterior, -((strokeWidth * GraphicalElement.clipperCoordinateScale) / 2f));
                closedStrokeOffsetter.Execute(ref closedExterior, ((strokeWidth * GraphicalElement.clipperCoordinateScale) / 2f));

                Clipper closedStrokeClipper = new Clipper();
                closedStrokeClipper.StrictlySimple = true;
                closedStrokeClipper.AddPaths(closedExterior, PolyType.ptSubject, true);
                closedStrokeClipper.AddPaths(closedInterior, PolyType.ptClip, true);

                bool closedStrokeClipSuccess = closedStrokeClipper.Execute(
                    ClipType.ctDifference,
                    closedStrokeTree,
                    fillRule,
                    fillRule
                );

                if (!closedStrokeClipSuccess) {
                    return false;
                }
            }

            return true;
        }

        public static bool ClipStencil(
            PolyTree subject,
            PolyTree stencil,
            out PolyTree result
        ) {
            Clipper stencilClipper = new Clipper();
            stencilClipper.StrictlySimple = true;

            result = new PolyTree();

            stencilClipper.AddPaths(Clipper.PolyTreeToPaths(subject), PolyType.ptSubject, true);
            stencilClipper.AddPaths(Clipper.PolyTreeToPaths(stencil), PolyType.ptClip, true);

            return stencilClipper.Execute(ClipType.ctIntersection, result, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
        }

        public static bool TriangulatePolyTree(
            PolyTree polyTree,
            Color color,
            ref List<Vector3> meshVertices,
            ref List<int> meshTriangles,
            ref List<Color> meshVertexColors
        ) {
            if (polyTree.ChildCount == 0) {
                if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_AND_WARNINGS) {
                    Debug.LogWarning("Polytree is empty. Cannot triangulate.");
                }
                return false;
            }

            foreach (PolyNode node in polyTree.Childs) {
                if (!RecurseAndTriangulatePolyTree(node, color, ref meshVertices, ref meshTriangles, ref meshVertexColors)) {
                    return false;
                }
            }

            return true;
        }

        static bool RecurseAndTriangulatePolyTree(
            PolyNode node,
            Color color,
            ref List<Vector3> meshVertices,
            ref List<int> meshTriangles,
            ref List<Color> meshVertexColors
        ) {
            Queue<PolyNode> recurseTo = new Queue<PolyNode>();

            if (node.IsHole) {
                // Assume that there are no holes as direct children of a hole

                foreach (PolyNode child in node.Childs) {
                    recurseTo.Enqueue(child);
                }

            } else {
                Polygon polygon = ConvertIntPointsToPolygon(Clipper.CleanPolygon(node.Contour));

                if (polygon == null) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_AND_WARNINGS) {
                        Debug.LogWarning("Building subject polygon failed. Skipping.");
                    }
                    return false;
                }

                foreach (PolyNode child in node.Childs) {
                    if (child.IsHole) {
                        Polygon hole = ConvertIntPointsToPolygon(Clipper.CleanPolygon(child.Contour));

                        if (hole == null) {
                            if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_AND_WARNINGS) {
                                Debug.LogWarning("Building hole polygon failed. Skipping.");
                            }
                            continue;

                        } else {
                            polygon.AddHole(hole);
                        }
                    }

                    if (child.ChildCount > 0) {
                        recurseTo.Enqueue(child);
                    }
                }

                try {
                    P2T.Triangulate(polygon);
                } catch (System.Exception e) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS) {
                        Debug.LogError("Polygon triangulation failed with an exception: " + e.Message);
                    }
                }

                if (polygon.Triangles.Count > 0) {
                    AppendToMeshData(polygon.Triangles, color, ref meshVertices, ref meshTriangles, ref meshVertexColors);
                } else {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_AND_WARNINGS) {
                        Debug.LogWarning("Skipped appending polygon to mesh");
                    }
                }
            }

            foreach (PolyNode child in recurseTo) {
                if (!RecurseAndTriangulatePolyTree(child, color, ref meshVertices, ref meshTriangles, ref meshVertexColors)) {
                    continue;
                }
            }

            return true;
        }

        static Polygon ConvertIntPointsToPolygon(
            List<IntPoint> path
        ) {
            if (path.Count < 3) {
                return null;
            }

            List<PolygonPoint> points = new List<PolygonPoint>();

            foreach (IntPoint contourPoint in path) {
                points.Add(ImportUtilities.ConvertToTriangulationPoint(contourPoint));
            }

            Polygon polygon = new Polygon(points);

            polygon.RemoveDuplicateNeighborPoints();
            polygon.Simplify();

            // Validate
            if (GlobalSettings.Get().logLevelInteger >= 2 && GlobalSettings.Get().extraDevelopementChecks) {
                Point2DList.PolygonError polygonCheckError = polygon.CheckPolygon();
                if (polygonCheckError != Point2DList.PolygonError.None) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_WARNINGS_AND_INFO) {
                        Debug.LogWarning("Polygon validity check reported a potential problem: " + polygonCheckError);
                    }
                }
            }

            return polygon;
        }

        static void AppendToMeshData(
            IList<DelaunayTriangle> triangles,
            Color color,
            ref List<Vector3> meshVertices,
            ref List<int> meshTriangles,
            ref List<Color> meshVertexColors
        ) {
            int meshVerticesOffset = meshVertices.Count;
            for (int i = 0; i < triangles.Count; i++) {
                DelaunayTriangle triangle = triangles[i];

                for (int j = 0; j < 3; j++) {
                    TriangulationPoint vertex = triangle.Points[j];
                    
                    meshVertices.Add(new Vector3(vertex.Xf, -vertex.Yf, 0f));
                    meshVertexColors.Add(color);
                }

                meshTriangles.Add(meshVerticesOffset + i * 3 + 0);
                meshTriangles.Add(meshVerticesOffset + i * 3 + 1);
                meshTriangles.Add(meshVerticesOffset + i * 3 + 2);
            }
        }
    }
}
