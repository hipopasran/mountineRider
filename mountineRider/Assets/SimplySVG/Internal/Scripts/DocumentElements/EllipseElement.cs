using UnityEngine;
using System.Collections.Generic;
using System;

namespace SimplySVG {
    public class EllipseElement : GraphicalElement {
        float cx = 0;
        float cy = 0;
        float rx = 0;
        float ry = 0;

        public EllipseElement() {

        }

        public override bool AddShapeAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutErrors = true;

            if (attributeName == "cx") {
                parsedWithoutErrors = float.TryParse(attributeValue, out cx);

            } else if (attributeName == "cy") {
                parsedWithoutErrors = float.TryParse(attributeValue, out cy);

            } else if (attributeName == "rx") {
                parsedWithoutErrors = float.TryParse(attributeValue, out rx);

            } else if (attributeName == "ry") {
                parsedWithoutErrors = float.TryParse(attributeValue, out ry);

            } else {
                return false;
            }

            if (!parsedWithoutErrors) {
                throw new Exception("Failed to parse Ellipse attribute " + attributeName + " with value " + attributeValue);
            }

            return true;
        }

        protected override List<ContourPath> BuildShape(ImportSettings options) {
            List<ContourPath> contourPaths = new List<ContourPath>();
            
            ContourPath contourPath = new ContourPath(
                true,
                MakeEllipsoidContourPoints(options, cx, cy, rx, ry, 0)
            );

            contourPaths.Add(contourPath);
            return contourPaths;
        }

        public static List<Vector2> MakeEllipsoidContourPoints(ImportSettings options, float cx, float cy, float rx, float ry, float angle = 0) {
            List<Vector2> path = new List<Vector2>();

            PathElement.MakePathPointDelegate makePoint = delegate (float t) {
                float a = 2f * Mathf.PI * t + angle;
                return new Vector2(
                    Mathf.Sin(a) * rx + cx,
                    Mathf.Cos(a) * ry + cy
                );
            };

            Vector2 startPoint = makePoint(0f);
            Vector2 halfPoint = makePoint(0.5f);

            // First half
            PathElement.DynamicallySubdivide(
                options,
                makePoint,
                0,
                0f,
                0.5f,
                startPoint,
                halfPoint,
                ref path
            );

            path.Add(halfPoint);

            // Second half
            PathElement.DynamicallySubdivide(
                options,
                makePoint,
                0,
                0.5f,
                1f,
                halfPoint,
                startPoint,
                ref path
            );

            path.Add(startPoint);

            return path;
        }
    }
}
