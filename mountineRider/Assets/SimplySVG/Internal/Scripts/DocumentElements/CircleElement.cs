using UnityEngine;
using System.Collections.Generic;
using System;

namespace SimplySVG {
    public class CircleElement : GraphicalElement {
        float cx = 0;
        float cy = 0;
        float r = 0;

        public CircleElement() {

        }

        public override bool AddShapeAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutErrors = true;

            if (attributeName == "cx") {
                parsedWithoutErrors = float.TryParse(attributeValue, out cx);

            } else if (attributeName == "cy") {
                parsedWithoutErrors = float.TryParse(attributeValue, out cy);

            } else if (attributeName == "r") {
                parsedWithoutErrors = float.TryParse(attributeValue, out r);

            } else {
                return false;
            }

            if (!parsedWithoutErrors) {
                throw new Exception("Failed to parse Circle attribute " + attributeName + " with value " + attributeValue);
            }

            return true;
        }

        protected override List<ContourPath> BuildShape(ImportSettings options) {
            List<ContourPath> contourPaths = new List<ContourPath>();

            ContourPath contourPath = new ContourPath(
                true,
                EllipseElement.MakeEllipsoidContourPoints(options, cx, cy, r, r, 0)
            );

            contourPaths.Add(contourPath);
            return contourPaths;
        }
    }
}
