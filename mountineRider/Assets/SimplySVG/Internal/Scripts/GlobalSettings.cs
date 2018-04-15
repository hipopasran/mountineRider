// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;

namespace SimplySVG {
    public class GlobalSettings : ScriptableObject {
        private static GlobalSettings current;
        private static string[] searchPaths = { "Assets" };

        public ImportSettings defaultImportSettings = null;
        public Material defaultMaterial = null;
		public LogLevel levelOfLog;
        public int maxUnsupportedFeatureWarningCount = 5;
        
        public bool extraDevelopementChecks = false;

        public int logLevelInteger {
			get{
				switch(levelOfLog){
				case LogLevel.CRITICALS:
					return 0;
				case LogLevel.ERRORS:
					return 1;
				case LogLevel.ERRORS_AND_WARNINGS:
					return 2;
				case LogLevel.ERRORS_WARNINGS_AND_INFO:
					return 3;
				default:
					return 2;
				}
			}
		}

        public static GlobalSettings Get() {
            if (current == null) {
#if UNITY_EDITOR
                string[] globalSettingsAssetPaths = AssetDatabase.FindAssets("t:GlobalSettings", searchPaths);

                if (globalSettingsAssetPaths.Length == 0) {
                    throw new System.Exception("GlobalSettings object is missing from the project! To create a new one, select SimplySVG -> \"Reset global settings\" from the main menu.");

                } else if (globalSettingsAssetPaths.Length > 1) {
                    throw new System.Exception("More than one GlobalSettings object exists in the project.");

                } else if (globalSettingsAssetPaths.Length == 1) {
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                    current = AssetDatabase.LoadAssetAtPath<GlobalSettings>(AssetDatabase.GUIDToAssetPath(globalSettingsAssetPaths[0]));
#else
                    current = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(globalSettingsAssetPaths[0]), typeof(GlobalSettings)) as GlobalSettings;
#endif
                }
                
                if (current == null) {
                    throw new System.Exception("Cannot find GlobalSettings asset. Use SimplySVG -> \"Reset global settings\" from the main menu.");
                }
#else
                Debug.LogWarning("GlobalSettings instance has not been set. Default settings will be used.");
                current = ScriptableObject.CreateInstance<GlobalSettings>();
#endif
            }

            return current;
        }
    }

	public enum LogLevel{
		CRITICALS,
		ERRORS,
		ERRORS_AND_WARNINGS,
		ERRORS_WARNINGS_AND_INFO
	}
}
