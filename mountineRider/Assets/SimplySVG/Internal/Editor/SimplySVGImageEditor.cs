// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SimplySVG {

    [CustomEditor(typeof(SimplySVGImage))]
    public class SimplySVGImageEditor : Editor {
        SimplySVGImage image;

        void OnEnable() {
            image = (SimplySVGImage)target;
        }

        override public void OnInspectorGUI() {
#if (UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
            string unityVersionNotSupportedMessage = "Unity UI is supported only for Unity 5.2 and up!";
            EditorGUILayout.HelpBox(unityVersionNotSupportedMessage, MessageType.Error);
            Debug.LogError(unityVersionNotSupportedMessage);
#else
            Undo.RecordObject(image, "Simply SVG Image");

            Mesh newMesh = (Mesh)EditorGUILayout.ObjectField("SVG Mesh", image.graphicMesh, typeof(Mesh), false);
            if (newMesh == null) {
                image.graphicMesh = newMesh;

            } else if (newMesh != image.graphicMesh) {
                // Check mesh validity
                Vector3[] positions = newMesh.vertices;
                Color32[] colors = newMesh.colors32;
                int[] triangles = newMesh.triangles;
                int vertexCount = positions.Length;

                if (
                    positions == null || positions.Length < 3 ||
                    triangles == null || triangles.Length < 3 ||
                    colors == null || colors.Length < 3 || colors.Length != vertexCount
                ) {
                    Debug.LogError(" The selected mesh (" + newMesh.name + ") cannot bu used with SimplySVGImage as it is missing geometry or vertex colors or contains otherwise invalid data.");

                } else {
                    image.graphicMesh = newMesh;
                }
            }
  
            image.color = EditorGUILayout.ColorField(new GUIContent("Color", "Tint color"), image.color);
            
            image.preserveAspectRatio = EditorGUILayout.Toggle("Preserve aspect ratio", image.preserveAspectRatio);
            image.raycastTarget = EditorGUILayout.Toggle("Raycast target", image.raycastTarget);
            if (image.raycastTarget) {
                image.useComplexHitCheck = EditorGUILayout.Toggle(new GUIContent("Use complex hit check", "On: Click hit checks are done against the graphical mesh\n\nOff: Click hit checks are done against the bounding box of the graphic"), image.useComplexHitCheck);
            }
            // Set rect size
            if (GUILayout.Button("Reset to native size")) {
                image.rectTransform.sizeDelta = image.graphicMesh.bounds.size;
            }

            // Dirty, filthy hack! Force canvas update, Unity style.
            // Apparently Canvas.ForceUpdateCanvases() does not work anymore...
            image.enabled = !image.enabled;
            image.enabled = !image.enabled;
#endif
        }

        void OnSceneGUI() {
            image.UpdateMaterialProperties();
        }

        [MenuItem("GameObject/UI/Simply SVG Image")]
        public static void CreateImageGameObject() {
            GameObject go = new GameObject("Simply SVG Image");
            Undo.RegisterCreatedObjectUndo(go, "Create Simply SVG Image");

            go.AddComponent<CanvasRenderer>();
            go.AddComponent<SimplySVGImage>();

            GameObject parent = Selection.activeGameObject;
            if (parent != null) {
                GameObjectUtility.SetParentAndAlign(go, parent);
            }

            Selection.activeGameObject = go;
        }
    }
}
