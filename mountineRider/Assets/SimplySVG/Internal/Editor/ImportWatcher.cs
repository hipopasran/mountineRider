// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimplySVG {
    public class ImportWatcher : AssetPostprocessor {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths
        ) {
            // Handle import and reimport
            foreach (string importedAsset in importedAssets) {
                if (!EditorUtilities.CheckAssetFileTypeByExtension(importedAsset, ".svg")) {
                    // Not an SVG document. Nothing to do.
                    continue;
                }


#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                Object svgFile = AssetDatabase.LoadAssetAtPath<Object>(importedAsset);
#else
                Object svgFile = AssetDatabase.LoadAssetAtPath(importedAsset, typeof(Object));
#endif

                string oldImportAssetPath = EditorUtilities.GetExistingImportedAssetPath(svgFile);
                // Try to use old settings

#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                ImportSettings importSettings = AssetDatabase.LoadAssetAtPath<ImportSettings>(oldImportAssetPath);
#else
                ImportSettings importSettings = AssetDatabase.LoadAssetAtPath(oldImportAssetPath, typeof(ImportSettings)) as ImportSettings;
#endif

                if (importSettings == null) {
                    // Create new settings
                    importSettings = ScriptableObject.CreateInstance<ImportSettings>();

                    // Try to load default values
                    ImportSettings defaultImportSettings = GlobalSettings.Get().defaultImportSettings;
                    if (defaultImportSettings != null) {
                        EditorUtility.CopySerialized(defaultImportSettings, importSettings);
                    }
                }

                importSettings.svgFile = svgFile;

                SimplySVGImporter importer = new SimplySVGImporter(svgFile, importSettings);

                // Import
                try {
                    importer.Import();
                } catch (System.Exception e) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS) {
                        Debug.LogError("An error occured while parsing and importing " + importedAsset + "\n\nException was:\n" + e.ToString() + "\n");
                    }
                    continue;
                }

                if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_WARNINGS_AND_INFO) {
                    Debug.Log("Imported successfully " + (svgFile != null ? svgFile.name : "null file"));
                }

                // Build
                try {
                    importer.Build();
                } catch (System.Exception e) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS) {
                        Debug.LogError("An error occured while building assets for " + importedAsset + "\n\nException was:\n" + e.ToString() + "\n");
                    }
                    continue;
                }

                if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_WARNINGS_AND_INFO) {
                    Debug.Log("Built successfully");
                }

                // Save
                try {
                    importer.SaveAssets();
                } catch (System.Exception e) {
                    if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS) {
                        Debug.LogError("An error occured while saving assets generated from " + importedAsset + "\n\nException was:\n" + e.ToString() + "\n");
                    }
                    continue;
                }

                if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS_WARNINGS_AND_INFO) {
                    Debug.Log("Asset saved successfully");
                }
            }

            // Handle move and rename
            for (int i = 0; i < movedAssets.Length; i++) {
                string movedSomethingNewPath = movedAssets[i];
                string movedSomethingOldPath = movedFromAssetPaths[i];

                if (!(movedSomethingNewPath.Contains("/") && movedSomethingNewPath.Contains("."))) {
                    continue;
                }

                string newDirectory = movedSomethingNewPath.Substring(
                    0,
                    movedSomethingNewPath.LastIndexOf('/') + 1
                );
                string newName = movedSomethingNewPath.Substring(
                    movedSomethingNewPath.LastIndexOf('/') + 1,
                    (movedSomethingNewPath.LastIndexOf('.') - 1) - movedSomethingNewPath.LastIndexOf('/')
                );

                if (EditorUtilities.CheckAssetFileTypeByExtension(movedSomethingNewPath, ".svg")) {
                    // The file moved was the main SVG file

                    string assetContainerPath = EditorUtilities.GetExistingImportedAssetPath(movedSomethingOldPath);
                    if (assetContainerPath != null) {
                        string toPath = newDirectory + newName + ".asset";

                        if (toPath == assetContainerPath) {
                            continue;
                        }

                        string validateMoveError = AssetDatabase.ValidateMoveAsset(assetContainerPath, toPath);
                        if (validateMoveError != "") {
                            Debug.LogError("Cannot move asset container. Error:\n" + validateMoveError);
                            continue;
                        }

                        AssetDatabase.MoveAsset(assetContainerPath, toPath);
                    }

                } else if (EditorUtilities.CheckAssetFileTypeByExtension(movedSomethingNewPath, ".asset")) {
                    // The moved file was some .asset container

#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                    ImportSettings importSettings = AssetDatabase.LoadAssetAtPath<ImportSettings>(movedSomethingNewPath);
#else
                    ImportSettings importSettings = AssetDatabase.LoadAssetAtPath(movedSomethingNewPath, typeof(ImportSettings)) as ImportSettings;
#endif
                    if (importSettings == null) {
                        continue;
                    }

                    // Check if this ImportSettings instance is actually linked to a SVG file
                    if (importSettings.svgFile == null) {
                        continue;
                    }

                    // The moved file was a SimplySVG asset container

                    string svgFileOldPath = AssetDatabase.GetAssetPath(importSettings.svgFile);
                    if (svgFileOldPath == null) {
                        continue;
                    }

                    string svgFileNewPath = newDirectory + newName + ".svg";

                    if (svgFileNewPath == svgFileOldPath) {
                        continue;
                    }

                    string validateMoveError = AssetDatabase.ValidateMoveAsset(svgFileOldPath, svgFileNewPath);
                    if (validateMoveError != "") {
                        if (GlobalSettings.Get().levelOfLog >= LogLevel.ERRORS) {
                            Debug.LogError("Cannot move SVG file. Error:\n" + validateMoveError);
                        }
                        continue;
                    }

                    AssetDatabase.MoveAsset(svgFileOldPath, svgFileNewPath);

                } else {
                    continue;
                }
            }
        }
    }
}
