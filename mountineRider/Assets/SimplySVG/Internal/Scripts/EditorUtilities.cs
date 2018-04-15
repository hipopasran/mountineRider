// Copyright © 2015 NordicEdu Ltd.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace SimplySVG {
    public static class EditorUtilities {
        public static string ReadTextFile(Object textAsset) {
            if (textAsset == null) {
                Debug.LogError("No SVG file has been selected");
                return null;
            }

            string svgFilePath = AssetDatabase.GetAssetPath(textAsset);

            if (svgFilePath == null) {
                Debug.LogError("Resolving SVG file path failed");
                return null;
            }

            System.IO.StreamReader reader = System.IO.File.OpenText(svgFilePath);
            string s = reader.ReadToEnd();
            reader.Dispose();
            return s;
        }

        public static string GetExistingImportedAssetPath(Object svgFile) {
            string svgFilePath = AssetDatabase.GetAssetPath(svgFile);

            if (svgFilePath == null) {
                return null;
            }

            return GetExistingImportedAssetPath(svgFilePath);
        }

        public static string GetExistingImportedAssetPath(string svgFilePath) {
            string assetPath = svgFilePath.Substring(0, svgFilePath.LastIndexOf(".svg")) + ".asset";
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
            Object obj = AssetDatabase.LoadAssetAtPath<ImportSettings>(assetPath);
#else
            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ImportSettings));
#endif
            if (obj == null) {
                // The path is invalid
                return null;
            }

            return assetPath;
        }

        public static List<GameObject> MakeGameObjectsForSVGAssets(Object[] svgFiles) {
            if (svgFiles.Length < 1) {
                return null;
            }

            List<GameObject> gos = new List<GameObject>(svgFiles.Length);

            foreach (Object svgFile in svgFiles) {
                string svgFilePath = AssetDatabase.GetAssetPath(svgFile);

                if (svgFilePath == null || !svgFilePath.EndsWith(".svg")) {
                    // Not an SVG file. Skip
                    continue;

                } else {
                    string assetContainerPath = GetExistingImportedAssetPath(svgFile);

                    // Try to find the settings mesh asset for this SVG file
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                    ImportSettings settings = AssetDatabase.LoadAssetAtPath<ImportSettings>(assetContainerPath);
#else
                    ImportSettings settings = AssetDatabase.LoadAssetAtPath(assetContainerPath, typeof(ImportSettings)) as ImportSettings;
#endif
                    GameObject rootObject = new GameObject(svgFile.name);

                    if (!settings.splitMeshesByLayers)
                    {
                        // Try to find the prebuilt mesh asset for this SVG file
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetContainerPath);
#else
                        Mesh mesh = AssetDatabase.LoadAssetAtPath(assetContainerPath, typeof(Mesh)) as Mesh;
#endif
                        if (mesh == null)
                        {
                            Debug.LogError("No mesh exists for this SVG file. It should be reimported. Path:\n" + svgFilePath);
                            continue;
                        }

                        // Construct a GameObject for displaying the SVG mesh
                        Undo.RegisterCreatedObjectUndo(rootObject, "Drag & drop SVG");

                        MeshFilter meshFilter = rootObject.AddComponent<MeshFilter>();
                        meshFilter.sharedMesh = mesh;

                        MeshRenderer meshRenderer = rootObject.AddComponent<MeshRenderer>();
                        meshRenderer.sharedMaterial = GlobalSettings.Get().defaultMaterial;
                        meshRenderer.receiveShadows = false;
                        meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
#if UNITY_4_6 || UNITY_4_7
                        meshRenderer.castShadows = false;
#else
                        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#endif

                        rootObject.AddComponent<RendererProperties>();

                        rootObject.AddComponent<PolygonCollider2D>();

                        ColliderHelper colliderHelper = rootObject.AddComponent<ColliderHelper>();
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                        colliderHelper.collisionShapeData = AssetDatabase.LoadAssetAtPath<CollisionShapeData>(assetContainerPath);
#else
                        colliderHelper.collisionShapeData = AssetDatabase.LoadAssetAtPath(assetContainerPath, typeof(CollisionShapeData)) as CollisionShapeData;
#endif
                        colliderHelper.autoUpdateOnAwake = true;
                        colliderHelper.UpdateColliderShape();
                    }
                    else
                    {
                        Object[] allAssetsFromSVG = AssetDatabase.LoadAllAssetsAtPath(assetContainerPath);
                        //generate meshes:
                        List<GameObject> layers = new List<GameObject>();
                        for (int i = 0; i < allAssetsFromSVG.Length; ++i)
                        {
                            if (allAssetsFromSVG[i].name.Contains("-mesh-layer-"))
                            {
                                // Debug.Log("add layer " + allAssetsFromSVG[i].name);

                                GameObject layerGO = new GameObject(allAssetsFromSVG[i].name);
                                layerGO.transform.SetParent(rootObject.transform, false);
                                layerGO.transform.localPosition = Vector3.zero;
                                layerGO.transform.localScale = Vector3.one;

                                Undo.RegisterCreatedObjectUndo(layerGO, "Drag & drop SVG layer");

                                MeshFilter meshFilter = layerGO.AddComponent<MeshFilter>();
                                meshFilter.sharedMesh = allAssetsFromSVG[i] as Mesh;

                                MeshRenderer meshRenderer = layerGO.AddComponent<MeshRenderer>();
                                meshRenderer.sharedMaterial = GlobalSettings.Get().defaultMaterial;
                                meshRenderer.receiveShadows = false;
                                meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
#if UNITY_4_6 || UNITY_4_7
                                meshRenderer.castShadows = false;
#else
                                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                                meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#endif
                                layerGO.AddComponent<RendererProperties>();
                                layers.Add(layerGO);
                            }
                        }
                        //add colliders
                        if (settings.SplitCollidersByLayers)
                        {
                            for (int i = 0; i < allAssetsFromSVG.Length; ++i)
                            {
                                if (allAssetsFromSVG[i].name.Contains("-collision-layer-"))
                                {
                                    //find right gameobject
                                    GameObject layerGO = layers.Find((go) => go.name == allAssetsFromSVG[i].name.Replace("-collision-layer-", "-mesh-layer-"));
                                    // Debug.Log("Add collider " + allAssetsFromSVG[i].name + " to mesh " + layerGO.name);
                                    if(layerGO != null)
                                    {
                                        layerGO.AddComponent<PolygonCollider2D>();
                                        ColliderHelper colliderHelper = layerGO.AddComponent<ColliderHelper>();
                                        colliderHelper.collisionShapeData = allAssetsFromSVG[i] as CollisionShapeData;
                                        colliderHelper.autoUpdateOnAwake = true;
                                        colliderHelper.UpdateColliderShape();
                                    }
                                }
                            }
                        }
                        else
                        {
                            rootObject.AddComponent<PolygonCollider2D>();

                            ColliderHelper colliderHelper = rootObject.AddComponent<ColliderHelper>();
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
                            colliderHelper.collisionShapeData = AssetDatabase.LoadAssetAtPath<CollisionShapeData>(assetContainerPath);
#else
                        colliderHelper.collisionShapeData = AssetDatabase.LoadAssetAtPath(assetContainerPath, typeof(CollisionShapeData)) as CollisionShapeData;
#endif
                            colliderHelper.autoUpdateOnAwake = true;
                            colliderHelper.UpdateColliderShape();
                        }
                    }
                    gos.Add(rootObject);
                }
            }

            return gos;
        }

        public static bool CheckAssetFileTypeByExtension(Object asset, string fileTypeExtension) {
            string path = AssetDatabase.GetAssetPath(asset);

            return CheckAssetFileTypeByExtension(path, fileTypeExtension);
        }

        public static bool CheckAssetFileTypeByExtension(string assetPath, string fileTypeExtension) {
            if (assetPath == null ||
                assetPath == "" ||
                fileTypeExtension == null ||
                fileTypeExtension == ""
            ) {
                return false;
            }

            if (!assetPath.EndsWith(fileTypeExtension)) {
                return false;
            }

            return true;
        }

        public static int SortingLayerField(string label, Renderer renderer) {       
            return SortingLayerField(new GUIContent(label), renderer);
        }

        public static int SortingLayerField(GUIContent guiContent, Renderer renderer) {
            SerializedObject targetRendererSerialized = new SerializedObject(renderer);
            SerializedProperty sortingLayerID = targetRendererSerialized.FindProperty("m_SortingLayerID");

            return SortingLayerField(guiContent, sortingLayerID);
        }

        /// <summary>
        /// Make a popup field for selecting a rendering layer.
        /// </summary>
        /// <param name="label">Lavel to be shown in the inspector view</param>
        /// <param name="sortingLayerID">Target serialized property. Find it with "serializedObject.FindProperty"</param>
        public static int SortingLayerField(GUIContent guiContent, SerializedProperty sortingLayerID) {
            System.Type[] editorTypes = typeof(Editor).Assembly.GetTypes();
            System.Type type = editorTypes.FirstOrDefault(
                t => t.Name == "EditorGUILayout"
            );
            
            System.Reflection.MethodInfo meth_SortingLayerField = type.GetMethod(
                "SortingLayerField",
                (System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic),
                null,
                new System.Type[] {
                    typeof(GUIContent),
                    typeof(SerializedProperty),
                    typeof(GUIStyle)
                },
                null
            );

            object[] parameters = new object[] {
                guiContent,
                sortingLayerID,
                EditorStyles.popup
            };

            meth_SortingLayerField.Invoke(null, parameters);

            return sortingLayerID.intValue;
        }

        public static bool GetSortingLayer(string name, out int id) {
            int[] layerIDs;
            string[] layerNames;
            GetSortingLayers(out layerIDs, out layerNames);

            for (int i = 0; i < layerNames.Length; i++) {
                if (layerNames[i] == name) {
                    id = layerIDs[i];
                    return true;
                }
            }

            id = 0;
            return false;
        }

        public static bool GetSortingLayer(int id, out string name) {
            int[] layerIDs;
            string[] layerNames;
            GetSortingLayers(out layerIDs, out layerNames);

            for (int i = 0; i < layerIDs.Length; i++) {
                if (layerIDs[i] == id) {
                    name = layerNames[i];
                    return true;
                }
            }

            name = null;
            return false;
        }

        public static void GetSortingLayers(out int[] ids, out string[] names) {
            System.Type internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);

            System.Reflection.PropertyInfo sortingLayerNamesProperty = internalEditorUtilityType.GetProperty(
                "sortingLayerNames",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
            );

            System.Reflection.PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty(
                "sortingLayerUniqueIDs",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
            );

            names = (string[])sortingLayerNamesProperty.GetValue(null, new object[0]);
            ids = (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
        }
    }
}
#endif