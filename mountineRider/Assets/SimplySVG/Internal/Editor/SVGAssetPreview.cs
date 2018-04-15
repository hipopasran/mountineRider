// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimplySVG {
    public class SVGAssetPreview : ObjectPreview {
        Dictionary<Object, Mesh> meshes;
        PreviewRenderUtility renderUtil;

        override public void Initialize(Object[] targets) {
            base.Initialize(targets);

            meshes = new Dictionary<Object, Mesh>(targets.Length);

            for (int i = 0; i < targets.Length; i++) {
                Object targetObj = targets[i];

                if (IsValidObjectType(targetObj)) {
                    Mesh mesh = LoadMesh(targetObj);
                    if (mesh == null && GlobalSettings.Get().levelOfLog == LogLevel.ERRORS_WARNINGS_AND_INFO) {
                        Debug.Log("Mesh is missing. Cannot show preview for SVG asset at " + AssetDatabase.GetAssetPath(target));

                    } else {
                        meshes.Add(targetObj, mesh);
                    }
                }
            }

            renderUtil = new PreviewRenderUtility();
#if UNITY_5_6
            renderUtil.m_Camera.orthographic = true;
            renderUtil.m_Camera.nearClipPlane = 0.1f;
            renderUtil.m_Camera.farClipPlane = 10f;
            renderUtil.m_Camera.transform.position = -Vector3.forward * 1f;
            renderUtil.m_Camera.transform.rotation = Quaternion.identity;
#else
            renderUtil.camera.orthographic = true;
            renderUtil.camera.nearClipPlane = 0.1f;
            renderUtil.camera.farClipPlane = 10f;
            renderUtil.camera.transform.position = -Vector3.forward * 1f;
            renderUtil.camera.transform.rotation = Quaternion.identity;
#endif
        }

        virtual protected bool IsValidObjectType(Object obj) {
            return true;
        }

        virtual protected Mesh LoadMesh(Object asset) {
            string assetPath = AssetDatabase.GetAssetPath(asset);
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
            return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
#else
            return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Mesh)) as Mesh;
#endif
        }

        override public GUIContent GetPreviewTitle() {
            return base.GetPreviewTitle();
        }

        override public string GetInfoString() {
            return base.GetInfoString();
        }

        override public bool HasPreviewGUI() {
            return meshes.ContainsKey(target);
        }

        override public void OnPreviewGUI(Rect r, GUIStyle background) {
            // Chech for invalid rectangle. This methdod is called with a nonsensical r when drawing multiple previews.
            if (r.width <= 0f || r.height <= 0f) {
                return;
            }

            renderUtil.BeginPreview(r, background);

            Mesh mesh;
            if (meshes.TryGetValue(target, out mesh) && mesh != null) {
#if UNITY_5_6
                renderUtil.m_Camera.orthographicSize = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y) / 2f;
                renderUtil.DrawMesh(meshes[target], Vector3.zero, Quaternion.identity, GlobalSettings.Get().defaultMaterial, 0);
                renderUtil.m_Camera.Render();
#else
                renderUtil.camera.orthographicSize = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y) / 2f;
                renderUtil.DrawMesh(meshes[target], Vector3.zero, Quaternion.identity, GlobalSettings.Get().defaultMaterial, 0);
                renderUtil.camera.Render();
#endif
            }

            Texture outputTexture = renderUtil.EndPreview();
            GUI.DrawTexture(r, outputTexture, ScaleMode.StretchToFill, true, 1f);
        }
    }

    [CustomPreview(typeof(CollisionShapeData))]
    public class CollisionShapeDataPreview : SVGAssetPreview {

    }

    [CustomPreview(typeof(ImportSettings))]
    public class ImportSettingsPreview : SVGAssetPreview {

    }

#if !(UNITY_4_6 || UNITY_4_7)
    [CustomPreview(typeof(DefaultAsset))]
#else
    //Unity 4.x doesn't support DefaultAsset
    //[CustomPreview(typeof())]
#endif
    public class SVGFilePreview : SVGAssetPreview {
        override protected Mesh LoadMesh(Object asset) {
            string assetPath = EditorUtilities.GetExistingImportedAssetPath(asset);

#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
            return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
#else
            return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Mesh)) as Mesh;
#endif
        }

        protected override bool IsValidObjectType(Object obj) {
            return EditorUtilities.CheckAssetFileTypeByExtension(obj, ".svg");
        }
    }
}
