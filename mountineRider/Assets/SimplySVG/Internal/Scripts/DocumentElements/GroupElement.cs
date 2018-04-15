using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace SimplySVG {
    public class GroupElement : SVGElement, SVGStylable, SVGTransformable {
        protected GraphicalAttributes localGraphicalAttributes;
        protected TransformAttributes localTransformAttributes;

        public GroupElement() {
            localGraphicalAttributes = new GraphicalAttributes();
            localTransformAttributes = new TransformAttributes();
        }

        public override bool AddAttribute(string attributeName, string attributeValue) {
            return
                base.AddAttribute(attributeName, attributeValue) ||
                AddStyleAttribute(attributeName, attributeValue) || 
                AddTransformAttribute(attributeName, attributeValue);
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
    }
}
