using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimplySVG {
    [CustomEditor(typeof(RendererProperties)), CanEditMultipleObjects]
    public class RendererPropertiesEditor : Editor {
        static bool showHelp = false;

        SerializedObject serializedRenderers;

        void OnEnable() {
            List<RendererProperties> targetRenderers = new List<RendererProperties>(targets.Length);
            foreach (Object targetObj in targets) {
                RendererProperties rendererProp = (RendererProperties)targetObj;

                rendererProp.OnEnable();
                targetRenderers.Add(rendererProp);
            }

            serializedRenderers = new SerializedObject(targetRenderers.ToArray());
        }

        override public void OnInspectorGUI() {
            serializedRenderers.Update();
            showHelp = EditorGUILayout.Foldout(showHelp, "Help");
            if (showHelp) {
                EditorGUILayout.HelpBox("Use this helper to select the rendering layer for the attached Renderer.\n\nYou can also manually set the sorting order. RenderSorter will override this.\n\nWhen used in combination with RenderSorter (must appear in a parent GameObject), the layer selection here must match that of the RenderSorter. Otherwise this Renderer will not be sorted.", MessageType.Info);
            }

            SerializedProperty sortingLayerIDProperty = serializedRenderers.FindProperty("layerId");
            EditorUtilities.SortingLayerField(new GUIContent("Sorting Layer", "Assing the Renderer to this layer. When used in combination with RenderSorter, the parent RenderSorter will sort this Renderer only if the layers match."), sortingLayerIDProperty);

            SerializedProperty sortingOrderProperty = serializedRenderers.FindProperty("order");
            EditorGUILayout.PropertyField(sortingOrderProperty, new GUIContent("Sorting Order", "Order in the selected layer. This value will be overridden if RenderSorter is used."));

            serializedRenderers.ApplyModifiedProperties();

            foreach (Object targetObj in targets)
            {
                RendererProperties rendererProp = (RendererProperties) targetObj;
                rendererProp.Save();
            }
        }
    }
}
