// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections.Generic;

namespace SimplySVG {
    public class CollisionShapeData : ScriptableObject {
        public List<Polygon> collisionPolygons;

        public void Add(List<Vector2> polygon) {
            if (collisionPolygons == null) {
                collisionPolygons = new List<Polygon>();
            }

            collisionPolygons.Add(new Polygon { points = polygon });
        }

        public void Clear() {
            collisionPolygons.Clear();
        }

        [System.Serializable]
        public class Polygon {
            public List<Vector2> points;
        }
    }
}
