using UnityEngine;
using System.Collections.Generic;

namespace SimplySVG {
    public interface SVGStylable {
        GraphicalAttributes GetLocalAttributes();
        bool AddStyleAttribute(string attributeName, string attributeValue);
    }
}
