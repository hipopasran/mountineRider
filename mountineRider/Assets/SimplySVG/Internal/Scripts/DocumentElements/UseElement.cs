using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SimplySVG {
    public class UseElement : SVGElement, SVGStylable, SVGTransformable {
        protected GraphicalAttributes localGraphicalAttributes;
        protected TransformAttributes localTransformAttributes;

        public SVGElement surrogateForElement;

        public UseElement() {
            localGraphicalAttributes = new GraphicalAttributes();
            localTransformAttributes = new TransformAttributes();
        }

        public override bool AddAttribute(string attributeName, string attributeValue) {
            return
                base.AddAttribute(attributeName, attributeValue) ||
                AddUseAttribute(attributeName, attributeValue) ||
                AddStyleAttribute(attributeName, attributeValue) ||
                AddTransformAttribute(attributeName, attributeValue);
        }

        public bool AddUseAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutErrors = true;

            if (attributeName == "xlink:href") {
                int idBeginsAt = attributeValue.IndexOf("#") + 1;

                parsedWithoutErrors = idBeginsAt >= 0;

                if (parsedWithoutErrors) {
                    string elementId = attributeValue.Substring(idBeginsAt);
                    SVGElement referencedElement = ownerDocument.GetElementById(elementId);

                    parsedWithoutErrors = referencedElement != null;

                    if (parsedWithoutErrors) {
                        surrogateForElement = referencedElement;
                    }
                }
            } else {
                return false;
            }

            if (!parsedWithoutErrors) {
                throw new Exception(
                    "Failed to process Use attribute " + attributeName +
                    " with value " + attributeValue
                );
            }

            return true;
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

        public override bool Triangulate(CascadeContext parentCascadeContext, ImportSettings options, ref List<Vector3> meshVertices, ref List<int> meshTriangles, ref List<Color> meshVertexColors) {
            CascadeContext cascadeContext = parentCascadeContext.GatherElement(this);

            return surrogateForElement.Triangulate(cascadeContext, options, ref meshVertices, ref meshTriangles, ref meshVertexColors);
        }
    }
}
