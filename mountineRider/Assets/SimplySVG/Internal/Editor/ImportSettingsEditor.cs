// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimplySVG {
    [CustomEditor(typeof(ImportSettings))]
    [CanEditMultipleObjects]
    public class ImportSettingsEditor : Editor {
        List<ImportSettings> importSettings;

        void OnEnable() {
            importSettings = new List<ImportSettings>(targets.Length);
            foreach (Object targetObj in targets) {
                importSettings.Add((ImportSettings)targetObj);
            }
        }

        override public void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.HelpBox("These settings control various properties of the SVG document import process. Hover your mouse over the fields for more information.", MessageType.Info);
            GUILayout.Space(16f);

            SerializedProperty scaleProperty = serializedObject.FindProperty("scale");
            EditorGUILayout.PropertyField(scaleProperty, new GUIContent("Scale", "Scaling multiplier of the SVG graphic.\n\nThis sets the conversion scale from SVG units to Unity units."));


            SerializedProperty splitMeshProperty = serializedObject.FindProperty("splitMeshesByLayers");
            if (!splitMeshProperty.boolValue)
            {
                SerializedProperty pivotProperty = serializedObject.FindProperty("pivot");
                EditorGUILayout.PropertyField(pivotProperty, new GUIContent("Pivot", "Pivot point is relative to the bounds of the graphic. [x: 0.5, y: 0.5] is the center."));
            }
            else
            {
                GUILayout.Space(18f);
            }
            GUILayout.Space(16f);

            SerializedProperty qualityProperty = serializedObject.FindProperty("minSubdivisionDistanceDelta");
            EditorGUI.BeginChangeCheck();
            float qualityDisplayValue = EditorGUILayout.Slider(new GUIContent("Quality", "High value results smoother curvers. A low value generates less triangles and rougher curves."), 1f / qualityProperty.floatValue, 0.00625f, 100f);
            if (EditorGUI.EndChangeCheck()) {
                qualityProperty.floatValue = 1f / qualityDisplayValue;
            }
            if (qualityProperty.hasMultipleDifferentValues) {
                GUIStyle valuesDifferStyle = new GUIStyle
                {
                    alignment = TextAnchor.MiddleRight,
                    fontStyle = FontStyle.Italic
                };

                EditorGUILayout.LabelField("Values differ!", valuesDifferStyle);
            }
            GUILayout.Space(8f);
            EditorGUILayout.PropertyField(splitMeshProperty, new GUIContent("Split meshes by layers", "Split Meshed by firts level of g-tags"));
            if (splitMeshProperty.boolValue)
            {
                SerializedProperty splitColliderProperty = serializedObject.FindProperty("_splitCollidersByLayers");
                EditorGUILayout.PropertyField(splitColliderProperty, new GUIContent("Split colliders by layers", "Split colliders by firts level of g-tags"));
            }
            else
            {
                GUILayout.Space(18f);
            }

            GUILayout.Space(8f);

            if (GlobalSettings.Get().defaultImportSettings == target) {
                EditorGUILayout.HelpBox("This instance has been selected as the global default. These settings will be copied to any SVG files imported in the future.\n\nNOTE: Changes to this instance will not affect already imported SVGs!", MessageType.Info);

            } else if (importSettings.Count == 1 && importSettings[0].svgFile == null) {
                EditorGUILayout.HelpBox("This instance is not connected to anything", MessageType.Error);
            } else {
                if (GUILayout.Button(new GUIContent("Reimport", "Reimport the document using current settings. The settings are not saved."), GUILayout.Height(32f))) {
                    foreach (ImportSettings settings in importSettings) {
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings.svgFile), ImportAssetOptions.ForceUpdate);
                    }
                }
            }

            if (GUILayout.Button(new GUIContent("Reset", "Reset the setting to the global default values. If no global default ImportSettings instance is selected in Global Settings, hardcoded default values are used."))) {
                for (int i = 0; i < importSettings.Count; i++) {
                    Undo.RecordObjects(importSettings.ToArray(), "Reset ImportSettings");

                    ResetToDefault(importSettings[i]);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void ResetToDefault(ImportSettings settings) {
            ImportSettings globalImportSettings = GlobalSettings.Get().defaultImportSettings;

            Object oldSvgFileReference = settings.svgFile;

            if (globalImportSettings == null) {
                // Use hardcoded default values

                ImportSettings temp = ScriptableObject.CreateInstance<ImportSettings>();
                EditorUtility.CopySerialized(temp, settings);
                Object.DestroyImmediate(temp);

            } else {
                // Use the global default settings

                EditorUtility.CopySerialized(globalImportSettings, settings);
                AssetDatabase.SaveAssets();
            }

            settings.svgFile = oldSvgFileReference; // Preserve the old SVG file association
            AssetDatabase.SaveAssets();
        }
    }
}
