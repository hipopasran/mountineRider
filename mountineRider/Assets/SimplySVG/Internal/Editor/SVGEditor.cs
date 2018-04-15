// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimplySVG {

#if !(UNITY_4_6 || UNITY_4_7)
    [CustomPreview(typeof(DefaultAsset))]
#else
    //Unity 4.x doesn't support DefaultAsset
    //[CustomPreview(typeof())]
#endif
    public class SVGEditor : Editor {

		override public void OnInspectorGUI() {

			if (EditorUtilities.CheckAssetFileTypeByExtension (Selection.objects [0], ".svg")) {

				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup(false);
				EditorGUILayout.HelpBox("SimplySVG automatically imports SVG files. You should see several different subassets in the Project view connected to this asset. You can also Drag and Drop any SVG files directly to the Scene view to use them.", MessageType.Info);
				//Show svg asset container:
				Object svgFile = Selection.objects [0];
				string oldImportAssetPath = EditorUtilities.GetExistingImportedAssetPath(svgFile);
                // Try to use old settings

#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                Object importSettings = AssetDatabase.LoadAssetAtPath<Object>(oldImportAssetPath);
#else
                Object importSettings = AssetDatabase.LoadAssetAtPath(oldImportAssetPath, typeof(Object));
#endif
                if (importSettings != null) {
					if(GUILayout.Button("Select the Mesh subasset of this SVG")){
						Selection.objects = new Object[]{importSettings};
					}
				}

				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup(true);

			} else {
				//DrawDefaultInspector();
			}
		}
	}
}