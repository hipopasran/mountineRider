using UnityEngine;
using System.Collections.Generic;
using System;

namespace SimplySVG {
    public class PolylineElement : GraphicalElement {
        List<Vector2> points;

        public PolylineElement() {
            points = new List<Vector2>();
        }

        public override bool AddShapeAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutErrors = true;

            if (attributeName == "points") {
                parsedWithoutErrors = ParsePoints(attributeValue);

            } else {
                return false;
            }

            if (!parsedWithoutErrors) {
                throw new Exception("Failed to parse Polyline attribute " + attributeName + " with value " + attributeValue);
            }

            return true;
        }

        protected override List<ContourPath> BuildShape(ImportSettings options) {
            List<ContourPath> contourPaths = new List<ContourPath>();

            contourPaths.Add(
                new ContourPath(
                    false,
                    // Copy the point list
                    points.GetRange(0, points.Count)
                )
            );

            return contourPaths;
        }

        bool ParsePoints(string data) {
            //split values by comma or wps or minus mark
            data = data.Replace("-", ",-");

            List<char> separators = new List<char>(ImportUtilities.wps);
            separators.Add(',');

            string[] coordinates = data.Split(separators.ToArray(), System.StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < coordinates.Length; i += 2) {
                Vector2 point = new Vector2(
                    float.Parse(coordinates[i]),
                    float.Parse(coordinates[i + 1])
                );

                points.Add(point);
            }

            return true;
        }
    }
}
