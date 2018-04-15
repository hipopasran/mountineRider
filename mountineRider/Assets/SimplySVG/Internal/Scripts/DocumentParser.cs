// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections.Generic;

namespace SimplySVG {
    public class DocumentParser {
        SVGDocument currentDocument;
        Stack<SVGElement> buildStack;

        public DocumentParser() {

        }

        public SVGDocument BeginDocument() {
            currentDocument = new SVGDocument();

            buildStack = new Stack<SVGElement>();
            buildStack.Push(currentDocument.rootElement);

            return currentDocument;
        }

        public void EndDocument() {

        }

        public bool BeginElement(string type) {
            CheckStack();

            SVGElement element;

            if (type == "g") {
                element = new GroupElement();

            } else if (type == "path") {
                element = new PathElement();

            } else if (type == "polygon") {
                element = new PolygonElement();

            } else if (type == "polyline") {
                element = new PolylineElement();

            } else if (type == "line") {
                element = new LineElement();

            } else if (type == "ellipse") {
                element = new EllipseElement();

            } else if (type == "circle") {
                element = new CircleElement();

            } else if (type == "rect") {
                element = new RectElement();

            } else if (type == "defs") {
                element = new DefsElement();

            } else if (type == "use") {
                element = new UseElement();

            } else if (type == "clipPath") {
                element = new ClipPathElement();

            } else {
                // The element was not recognized.
                buildStack.Push(null);
                return false;
            }

            // The current element was recognized

            // Check if the current document branch is of unrecognized type
            if (buildStack.Peek() == null) {
                // Push null to the stack to keep it in sync with the parsed document
                buildStack.Push(null);
                return true;
            }

            buildStack.Peek().AddChild(element);
            buildStack.Push(element);

            return true;
        }

        public void EndElement() {
            buildStack.Pop();
        }

        public bool AddAttribute(string attrName, string attrValue) {
            CheckStack();

            SVGElement target = buildStack.Peek();

            if (target == null) {
                // Do not raise and error as the context is unknown anyway
                return true;
            }

            return target.AddAttribute(attrName, attrValue);
        }

        void CheckStack() {
            if (buildStack == null) {
                throw new System.Exception("Build stack is not initalized. Document may be malformed.");
            }

            if (buildStack.Count < 1) {
                throw new System.Exception("Build stack is empty. Document may be malformed.");
            }
        }
    }
}
