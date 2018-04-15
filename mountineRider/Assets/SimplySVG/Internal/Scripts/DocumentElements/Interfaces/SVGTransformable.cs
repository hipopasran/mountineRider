using UnityEngine;
using System.Collections;

namespace SimplySVG {
    public interface SVGTransformable {
        TransformAttributes GetLocalTransformation();
        bool AddTransformAttribute(string attributeName, string attributeValue);
    }
}
