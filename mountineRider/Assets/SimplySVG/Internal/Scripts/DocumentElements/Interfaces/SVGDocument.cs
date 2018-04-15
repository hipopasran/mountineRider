using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace SimplySVG {
    public class SVGDocument {
        public SVGElement rootElement;

        public ImportSettings importSettings;

        Dictionary<string, SVGElement> elementsById;

        public SVGDocument() {
            rootElement = new GroupElement();
            rootElement.ownerDocument = this;

            elementsById = new Dictionary<string, SVGElement>();
        }

        public bool AddElementToIdIndex(SVGElement element) {
            if (
                element == null ||
                element.id == null ||
                element.id == "" ||
                element.ownerDocument != this
            ) {
                return false;
            }

            if (elementsById.ContainsKey(element.id)) {
                return false;
            }

            elementsById.Add(element.id, element);

            return true;
        }

        public SVGElement GetElementById(string id) {
            SVGElement element;
            bool elementFound = elementsById.TryGetValue(id, out element);
            
            if (!elementFound) {
                return null;
            }

            return element;
        }

        public bool Triangulate(
            ImportSettings options,
            ref List<Vector3> meshVertices,
            ref List<int> meshTriangles,
            ref List<Color> meshVertexColors
        ) {
            importSettings = options;

            return rootElement.Triangulate(
                new CascadeContext(),
                options,
                ref meshVertices,
                ref meshTriangles,
                ref meshVertexColors
            );
        }

        public string GetRootID()
        {
            // Debug.Log("root id " + this.rootElement.id + " count: " + this.rootElement.children.Count);
            if (this.rootElement.children.Count > 0)
            {
                // Debug.Log("root.child[0] id " + this.rootElement.children[0].id);
                return this.rootElement.children[0].id;
            }
            return this.rootElement.id;
        }
    }
}
