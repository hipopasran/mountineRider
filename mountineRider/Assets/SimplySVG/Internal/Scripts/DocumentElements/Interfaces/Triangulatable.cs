using UnityEngine;
using System.Collections.Generic;

namespace SimplySVG {
    public interface Triangulatable {
        bool Triangulate(
            CascadeContext parentCascadeContext,
            ImportSettings options,
            ref List<Vector3> meshVertices,
            ref List<int> meshTriangles,
            ref List<Color> meshVertexColors
        );
    }
}
