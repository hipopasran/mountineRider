// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections.Generic;

namespace SimplySVG {
    [RequireComponent(typeof(PolygonCollider2D))]
    public class ColliderHelper : MonoBehaviour {
        public CollisionShapeData collisionShapeData;
        public bool autoUpdateOnAwake = true;

        PolygonCollider2D polygonCollider;

        public void UpdateColliderShape() {
            if (collisionShapeData == null) {
                Debug.LogError("No collision shape data selected");
                return;
            }

            if (polygonCollider == null) {
                polygonCollider = GetComponent<PolygonCollider2D>();

                if (polygonCollider == null) {
                    polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
                }
            }

            polygonCollider.pathCount = collisionShapeData.collisionPolygons.Count;

            for (int i = 0; i < collisionShapeData.collisionPolygons.Count; i++) {
                polygonCollider.SetPath(i, collisionShapeData.collisionPolygons[i].points.ToArray());
            }
        }

        void Awake() {
            if (autoUpdateOnAwake) {
                UpdateColliderShape();
            }
        }
    }
}
