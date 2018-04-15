using UnityEngine;
using System.Collections.Generic;
using System;
using ClipperLib;

namespace SimplySVG {
    public class RectElement : GraphicalElement {
        float x = 0;
        float y = 0;
        float width = 0;
        float height = 0;

        public RectElement() {

        }

        public override bool AddShapeAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutErrors = true;

            if (attributeName == "x") {
                parsedWithoutErrors = float.TryParse(attributeValue, out x);

            } else if (attributeName == "y") {
                parsedWithoutErrors = float.TryParse(attributeValue, out y);

            } else if (attributeName == "width") {
                parsedWithoutErrors = float.TryParse(attributeValue, out width);

            } else if (attributeName == "height") {
                parsedWithoutErrors = float.TryParse(attributeValue, out height);

            } else {
                // The attribute was not recognized
                return false;
            }

            if (!parsedWithoutErrors) {
                throw new Exception(
                    "Failed to parse Rect attribute " + attributeName + 
                    " with value " + attributeValue
                );
            }

            return true;
        }

        protected override List<ContourPath> BuildShape(ImportSettings options) {
            List<ContourPath> contourPaths = new List<ContourPath>();

            ContourPath contourPath = new ContourPath(true);
            contourPaths.Add(contourPath);

            contourPath.path.Add(
                new Vector2(
                    x,
                    y
                )
            );

            contourPath.path.Add(
                new Vector2(
                    x + width,
                    y
                )
            );

            contourPath.path.Add(
                new Vector2(
                    x + width,
                    y + height
                )
            );

            contourPath.path.Add(
                new Vector2(
                    x,
                    y + height
                )
            );

            return contourPaths;
        }
    }
}
