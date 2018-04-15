// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace SimplySVG {
    [CustomEditor(typeof(GlobalSettings))]
    public class GlobalSettingsEditor : Editor {
        GlobalSettings settings;

        void OnEnable() {
            settings = (GlobalSettings)target;
        }

		public override void OnInspectorGUI () {
            serializedObject.Update();
            
            EditorGUILayout.HelpBox ("These are global settings of importing any SVG objects. Use Mouse hover tool tips to get more info on fields.", MessageType.Info);

            //Default settings
            SerializedProperty defaultImportSettingsProperty = serializedObject.FindProperty("defaultImportSettings");
            EditorGUILayout.PropertyField(defaultImportSettingsProperty, new GUIContent("Default Import Settings", "Settings that will be set to any new SVG object that will be imported."));

            //Default material
            SerializedProperty defaultMaterialProperty = serializedObject.FindProperty("defaultMaterial");
            EditorGUILayout.PropertyField(defaultMaterialProperty, new GUIContent("Default Material", "Material that will be set to a new MeshRender when you drag and drop any svg mesh to scene."));

            //Log level
            SerializedProperty levelOfLogProperty = serializedObject.FindProperty("levelOfLog");
            EditorGUILayout.PropertyField(levelOfLogProperty, new GUIContent("Logging Level", "Level of cruciality of log messages that will be included in the logs."));

            SerializedProperty maxUnsupportedFeatureWarningCountProperty = serializedObject.FindProperty("maxUnsupportedFeatureWarningCount");
            EditorGUILayout.PropertyField(maxUnsupportedFeatureWarningCountProperty, new GUIContent("Max unsupported feature warning count", "Limit the number of unsuported SVG feature warnings given if such are found during SVG document import.\n\nDisable the warnings by setting this to 0."));

            GUILayout.Space(8f);

            if (GUILayout.Button(new GUIContent("Reset", "Resets the settings back to their default values. Also deletes other GlobalSettins instances from the project."))) {
                ResetGlobalSettings(settings);
            }

            serializedObject.ApplyModifiedProperties();
		}

        public void ResetGlobalSettings(GlobalSettings resetTarget) {
            Undo.RecordObject(resetTarget, "Reset Global Settings");

            // Create new default settings
            GlobalSettings temp = ScriptableObject.CreateInstance<GlobalSettings>();
            EditorUtility.CopySerialized(temp, resetTarget);
            Object.DestroyImmediate(temp);

            AssetDatabase.SaveAssets();
        }
    }
}
