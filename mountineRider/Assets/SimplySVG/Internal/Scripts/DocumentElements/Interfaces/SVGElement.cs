using UnityEngine;
using System.Collections.Generic;
using System;

namespace SimplySVG {
    [System.Serializable]
    public abstract class SVGElement : Triangulatable {
        // Own properties
        public string id;

        // Structure
        public SVGDocument ownerDocument;
        public SVGElement parent;
        public List<SVGElement> children;

        public SVGElement() {
            children = new List<SVGElement>();
        }

        public virtual bool AddAttribute(string attributeName, string attributeValue) {
            bool parsedWithoutError = true;
            if (attributeName == "id") {
                id = attributeValue;

                parsedWithoutError = ownerDocument.AddElementToIdIndex(this);

            } else {
                return false;
            }

            if (!parsedWithoutError) {
                throw new Exception(
                    "Failed to parse Core attribute " + attributeName +
                    " with value " + attributeValue
                );
            }

            return true;
        }

        public void AddChild(SVGElement element) {
            element.ownerDocument = ownerDocument;

            element.parent = this;
            children.Add(element);
        }

        public virtual bool Triangulate(
            CascadeContext parentCascadeContext,
            ImportSettings options,
            ref List<Vector3> meshVertices,
            ref List<int> meshTriangles,
            ref List<Color> meshVertexColors
        ) {
            CascadeContext cascadeContext = parentCascadeContext.GatherElement(this);

            for (int i = 0; i < children.Count; i++) {
                if (!children[i].Triangulate(
                    cascadeContext,
                    options,
                    ref meshVertices,
                    ref meshTriangles,
                    ref meshVertexColors
                )) {
                    // Ignore errors and continue...
                }
            }

            return true;
        }
    }
}
