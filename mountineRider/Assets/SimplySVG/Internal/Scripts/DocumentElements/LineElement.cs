using UnityEngine;
using System.Collections.Generic;
using System;

namespace SimplySVG {
    public class LineElement : GraphicalElement {
        float x1 = 0;
        float y1 = 0;
        float x2 = 0;
        float y2 = 0;

        public LineElement() {

        }

        public override bool AddShapeAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutErrors = true;

            if (attributeName == "x1") {
                parsedWithoutErrors = float.TryParse(attributeValue, out x1);

            } else if (attributeName == "y1") {
                parsedWithoutErrors = float.TryParse(attributeValue, out y1);

            } else if (attributeName == "x2") {
                parsedWithoutErrors = float.TryParse(attributeValue, out x2);

            } else if (attributeName == "y2") {
                parsedWithoutErrors = float.TryParse(attributeValue, out y2);

            } else {
                return false;
            }

            if (!parsedWithoutErrors) {
                throw new Exception("Failed to parse Line attribute " + attributeName + " with value " + attributeValue);
            }

            return true;
        }

        protected override List<ContourPath> BuildShape(ImportSettings options) {
            List<ContourPath> contourPaths = new List<ContourPath>();
            ContourPath contourPath = new ContourPath(false);

            contourPath.path.Add(
                new Vector2(x1, y1)
            );

            contourPath.path.Add(
                new Vector2(x2, y2)
            );

            contourPaths.Add(contourPath);
            return contourPaths;
        }
    }
}
