// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SimplySVG {
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/SimplySVG Image")]
    public class SimplySVGImage : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter {
        public Mesh graphicMesh;

        public bool preserveAspectRatio = false;
        public bool useComplexHitCheck = true;

        public float flexibleHeight { get { return -1f; } }

        public float flexibleWidth { get { return -1f; } }

        public int layoutPriority { get { return 0; } }

        public float minHeight { get { return 0f; } }

        public float minWidth { get { return 0f; } }

        public float preferredHeight {
            get {
                if (graphicMesh != null)
                    return graphicMesh.bounds.size.y;
                return 100f;
            }
        }

        public float preferredWidth {
            get {
                if (graphicMesh != null)
                    return graphicMesh.bounds.size.x;
                return 100f;
            }
        }

        public void CalculateLayoutInputHorizontal() { }

        public void CalculateLayoutInputVertical() { }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera) {
            if (!useComplexHitCheck) {
                return true;
            }

            Vector2 pointInGraphicRectSpace;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out pointInGraphicRectSpace);

            Vector2 pointInGraphicSpace = new Vector2(
                (pointInGraphicRectSpace.x / rectTransform.rect.width) * graphicMesh.bounds.size.x,
                (pointInGraphicRectSpace.y / rectTransform.rect.height) * graphicMesh.bounds.size.y
            );

            bool hit = false;

            int[] tris = graphicMesh.triangles;
            Vector3[] verts = graphicMesh.vertices;

            for (int i = 0; i < tris.Length; i += 3) {
                if (GeneralUtilities.PointInsideTriangle(
                    pointInGraphicSpace,
                    verts[tris[i + 0]],
                    verts[tris[i + 1]],
                    verts[tris[i + 2]]
                )) {
                    hit = true;
                    break;
                }
            }

            return hit;
        }
        
        override protected void OnEnable() {
            SetAllDirty();
            Canvas.ForceUpdateCanvases();

            base.OnEnable();
        }

#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
#if UNITY_5_2
        protected override void OnPopulateMesh(Mesh m) {
            
            if (graphicMesh == null) {
                return;
            }

            Vector2 transDims = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
            Vector2 meshDims = graphicMesh.bounds.size;

            Vector2 scale = new Vector2(
                transDims.x / meshDims.x,
                transDims.y / meshDims.y
            );

            if (preserveAspectRatio) {
                float min = Mathf.Min(
                    scale.x,
                    scale.y
                );

                scale.x = min;
                scale.y = min;
            }

            Vector3[] positions = graphicMesh.vertices;
            Color32[] colors = graphicMesh.colors32;
            int[] triangles = graphicMesh.triangles;
            using (VertexHelper vh = new VertexHelper())
            {
                for (int i = 0; i < triangles.Length; i = i + 3)
                {

                    for (int j = 0; j < 3; j++)
                    {
                        Vector3 position = positions[i + j] - graphicMesh.bounds.center;
                        position.x *= scale.x;
                        position.y *= scale.y;

                        vh.AddVert(
                            position,
                            colors[i + j],
                            Vector2.zero
                        );
                    }

                    vh.AddTriangle(
                        triangles[i + 0],
                        triangles[i + 1],
                        triangles[i + 2]
                    );
                }
                vh.FillMesh(m);
            }
        }
#else
        protected override void OnPopulateMesh(VertexHelper vh) {

            vh.Clear();

            if (graphicMesh == null) {
                return;
            }

            Vector2 transDims = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
            Vector2 meshDims = graphicMesh.bounds.size;

            Vector2 scale = new Vector2(
                transDims.x / meshDims.x,
                transDims.y / meshDims.y
            );

            if (preserveAspectRatio) {
                float min = Mathf.Min(
                    scale.x,
                    scale.y
                );

                scale.x = min;
                scale.y = min;
            }

            Vector3[] positions = graphicMesh.vertices;
            Color32[] colors = graphicMesh.colors32;
            int[] triangles = graphicMesh.triangles;
            
            for (int i = 0; i < triangles.Length; i = i + 3) {

                for (int j = 0; j < 3; j++) {
                    Vector3 position = positions[i + j] - graphicMesh.bounds.center;
                    position.x *= scale.x;
                    position.y *= scale.y;

                    vh.AddVert(
                        position,
                        colors[i + j],
                        Vector2.zero
                    );
                }

                vh.AddTriangle(
                    triangles[i + 0],
                    triangles[i + 1],
                    triangles[i + 2]
                );
            }
        }
#endif
#endif

        public void UpdateMaterialProperties() {
            if (!color.Equals(canvasRenderer.GetColor())) {
                canvasRenderer.SetColor(color);
            }
        }

        void Update() {
            UpdateMaterialProperties();
        }
    }
}
