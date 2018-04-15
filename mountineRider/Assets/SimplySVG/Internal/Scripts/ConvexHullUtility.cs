// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections.Generic;

namespace SimplySVG {
    public static class ConvexHullUtility {
        static float PointLocation(Vector2 A, Vector2 B, Vector2 P) {
            float cp1 = (B.x - A.x) * (P.y - A.y) - (B.y - A.y) * (P.x - A.x);
            return (cp1 > 0f) ? 1f : -1f;
        }

        static float Distance(Vector2 A, Vector2 B, Vector2 C) {
            float ABx = B.x - A.x;
            float ABy = B.y - A.y;
            float num = ABx * (A.y - C.y) - ABy * (A.x - C.x);
            if (num < 0f)
                num = -num;
            return num;
        }

        static void HullSet(Vector2 A, Vector2 B, List<Vector2> set, List<Vector2> hull) {
            int insertPosition = hull.IndexOf(B);
            if (set.Count == 0)
                return;
            if (set.Count == 1) {
                Vector2 p = set[0];
                set.Remove(p);
                hull.Insert(insertPosition, p);
                return;
            }
            float dist = float.MinValue;
            int furthestPoint = -1;
            for (int i = 0; i < set.Count; i++) {
                Vector2 p = set[i];
                float distance = Distance(A, B, p);
                if (distance > dist) {
                    dist = distance;
                    furthestPoint = i;
                }
            }
            Vector2 P = set[furthestPoint];
            set.RemoveAt(furthestPoint);
            hull.Insert(insertPosition, P);

            // Determine who's to the left of AP
            List<Vector2> leftSetAP = new List<Vector2>();
            for (int i = 0; i < set.Count; i++) {
                Vector2 M = set[i];
                if (PointLocation(A, P, M) == 1) {
                    leftSetAP.Add(M);
                }
            }

            // Determine who's to the left of PB
            List<Vector2> leftSetPB = new List<Vector2>();
            for (int i = 0; i < set.Count; i++) {
                Vector2 M = set[i];
                if (PointLocation(P, B, M) == 1) {
                    leftSetPB.Add(M);
                }
            }

            HullSet(A, P, leftSetAP, hull);
            HullSet(P, B, leftSetPB, hull);
        }

        public static List<Vector2> QuickHull(List<Vector2> points) {
            List<Vector2> convexHull = new List<Vector2>();

            if (points.Count < 3)
                return points;

            // find extremals
            int minPoint = -1;
            int maxPoint = -1;

            float minX = float.MaxValue;
            float maxX = float.MinValue;

            for (int i = 0; i < points.Count; i++) {
                if (points[i].x < minX) {
                    minX = points[i].x;
                    minPoint = i;
                }
                if (points[i].x > maxX) {
                    maxX = points[i].x;
                    maxPoint = i;
                }
            }
            Vector2 A = points[minPoint];
            Vector2 B = points[maxPoint];
            convexHull.Add(A);
            convexHull.Add(B);
            points.Remove(A);
            points.Remove(B);

            List<Vector2> leftSet = new List<Vector2>();
            List<Vector2> rightSet = new List<Vector2>();

            for (int i = 0; i < points.Count; i++) {
                Vector2 p = points[i];
                if (PointLocation(A, B, p) == -1) {
                    leftSet.Add(p);
                } else {
                    rightSet.Add(p);
                }
            }
            HullSet(A, B, rightSet, convexHull);
            HullSet(B, A, leftSet, convexHull);

            return convexHull;
        }

        public static List<Vector2> QuickHull(List<Vector3> points) {
            List<Vector2> points2 = new List<Vector2>();
            for (int i = 0; i < points.Count; i++) {
                points2.Add(new Vector2(points[i].x, points[i].y));
            }

            return QuickHull(points2);
        }
    }
}
