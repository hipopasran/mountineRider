// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
//using System;

namespace SimplySVG
{
    public class SimplySVGImporter
    {
        public string name = null;
        public ImportSettings importSettings = null;

        private Object svgFile;
        public SVGDocument document = null;
        public List<SVGDocument> svgDocumentLayers = new List<SVGDocument>();
        public string svgData;

        // Results
        public Mesh mesh = null;
        public List<Mesh> meshLayers = new List<Mesh>();
        public List<string> names = new List<string>();
        public CollisionShapeData collisionShapeData = null;
        public List<CollisionShapeData> collisionShapeDataLayers = new List<CollisionShapeData>();

        public string errors = "";

        //Warning handler
        //key=error, value=lines of document
        private Dictionary<string, List<int>> unsupportedElements = null;

#if UNITY_EDITOR
        int unsupportedFeatureWarningCount = 0;

        /// <summary>
        /// Constructor when in EDITOR
        /// </summary>
        /// <param name="svgFile"></param>
        /// <param name="settings"></param>
        public SimplySVGImporter(Object svgFile, ImportSettings settings = null)
            : this(GetSVGDataFromFile(svgFile), svgFile.name, settings)
        {
            this.svgFile = svgFile;
        }
#endif
        /// <summary>
        /// Constructor when in RUNTIME. Remember to call Import() and Build() methods after construction.
        /// </summary>
        /// <param name="svgData">text/xml content of a svg file</param>
        /// <param name="name">Name of the object</param>
        /// <param name="settings">Quality settings used while importing</param>
        public SimplySVGImporter(string svgData, string name, ImportSettings settings = null)
        {
            this.svgData = svgData;
            this.name = name;

            importSettings = settings;
            if (importSettings == null)
            {
                importSettings = ScriptableObject.CreateInstance<ImportSettings>();
            }
        }

#if UNITY_EDITOR
        private static string GetSVGDataFromFile(Object svgFile)
        {
            return EditorUtilities.ReadTextFile(svgFile);
        }
#endif

        public void Import()
        {

            if (string.IsNullOrEmpty(this.svgData))
            {
                Debug.LogError("SVG data was null or empty");
                return;
            }

            DocumentParser docBuilder = new DocumentParser();
            List<DocumentParser> docBuilderLayers = new List<DocumentParser>();
            DocumentParser currentDocBuilderLayer = null;

            XmlReaderSettings readerSettings = new XmlReaderSettings();

#if UNITY_EDITOR || !(UNITY_WSA || UNITY_WSA_8_0 || UNITY_WSA_8_1 || UNITY_WSA_10_0)
            readerSettings.ProhibitDtd = false;
            readerSettings.XmlResolver = null;
#else
            readerSettings.DtdProcessing = DtdProcessing.Ignore;
#endif
            int groupDepth = 0;
            using (XmlReader reader = XmlReader.Create(new StringReader(this.svgData), readerSettings))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "svg")
                            {
                                document = docBuilder.BeginDocument();
                            }
                            else
                            {
                                string type = reader.Name;
                                // Check if the element is self terminating
                                bool elementIsEmpty = reader.IsEmptyElement;

                                bool elementRecognized = docBuilder.BeginElement(type);

                                if (type == "g")
                                {
                                    ++groupDepth;
                                }

                                if (currentDocBuilderLayer == null)
                                {
                                    if (type == "g")
                                    {
                                        currentDocBuilderLayer = new DocumentParser();
                                        docBuilderLayers.Add(currentDocBuilderLayer);
                                        this.svgDocumentLayers.Add(currentDocBuilderLayer.BeginDocument());
                                        currentDocBuilderLayer.BeginElement(type); //beging with group
                                    }
                                    else
                                    {
                                        // Debug.LogWarning("Can't add type of " + type + " before layer is started");
                                    }
                                }
                                else
                                {
                                    currentDocBuilderLayer.BeginElement(type);
                                }

                                if (!elementRecognized)
                                {
                                    WarnAboutUnsupportedFeature(type + " element", reader);
                                    if (elementIsEmpty)
                                    {
                                        docBuilder.EndElement();
                                        if (currentDocBuilderLayer != null)
                                        {
                                            currentDocBuilderLayer.EndElement();
                                        }
                                    }
                                    continue;
                                }
                                else
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        string attributeName = reader.Name;
                                        string value = reader.Value;

                                        if (!docBuilder.AddAttribute(attributeName, value))
                                        {
                                            WarnAboutUnsupportedFeature(attributeName + " attribute", reader);
                                        }

                                        if (currentDocBuilderLayer != null)
                                        {
                                            currentDocBuilderLayer.AddAttribute(attributeName, value);
                                        }
                                    }
                                }

                                if (elementIsEmpty)
                                {
                                    docBuilder.EndElement();
                                    if (currentDocBuilderLayer != null)
                                    {
                                        currentDocBuilderLayer.EndElement();
                                    }
                                }
                            }
                            break;

                        case XmlNodeType.Text:
                            break;

                        case XmlNodeType.XmlDeclaration:
                            break;

                        case XmlNodeType.ProcessingInstruction:
                            break;

                        case XmlNodeType.Comment:
                            break;

                        case XmlNodeType.EndElement:
                            if (reader.Name == "svg")
                            {
                                docBuilder.EndDocument();
                            }
                            else
                            {
                                if (reader.Name == "g")
                                {
                                    --groupDepth;
                                    if (groupDepth == 0)
                                    {
                                        currentDocBuilderLayer.EndElement();
                                        currentDocBuilderLayer.EndDocument();
                                        currentDocBuilderLayer = null;
                                    }
                                }
                                if (currentDocBuilderLayer != null)
                                {
                                    currentDocBuilderLayer.EndElement();
                                }
                                docBuilder.EndElement();
                            }
                            break;
                    }
                }
            }
            //Check if unsupported features needs a show
            ShowUnsupportedFeatureIfNeeded();
        }

        private void ShowUnsupportedFeatureIfNeeded()
        {
            if (unsupportedElements != null)
            {
                StringBuilder sb = new StringBuilder("Unsupported SVG features detected while importing the SVG document at:\n" + this.name + "\n\nRead the documentation and check your graphics production software's SVG exporter settings. Unless fixed, the graphic may not appear in Unity as intended.\n\nUnsupported features are:\n\n");
                int warnings = 0;
                int maxWarnings = GlobalSettings.Get().maxUnsupportedFeatureWarningCount;
                if (maxWarnings == 0)
                {
                    unsupportedElements = null;

                    return;
                }
                foreach (KeyValuePair<string, List<int>> pair in unsupportedElements)
                {

                    if (++warnings > maxWarnings)
                    {
                        sb.Append("Showing only first " + maxWarnings + " warnings...\n");
                        break;
                    }

                    sb.Append(pair.Key + " at lines:");
                    int maxLines = 6;
                    for (int i = 0; i < pair.Value.Count; ++i)
                    {
                        sb.Append((i == 0 ? " " : ", ") + pair.Value[i]);
                        if(i >= maxLines)
                        {
                            int left = (pair.Value.Count - maxLines) - 1;
                            if(left > 0)
                            {
                                sb.Append(" and " + left + " more lines.");
                            }
                            break;
                        }
                    }
                    sb.Append("\n\n");
                }
#if UNITY_EDITOR
                if (!EditorUtility.DisplayDialog("Use of unsupported SVG feature detected!", sb.ToString(), "Ok", "Disable warnings globally"))
                {
                    GlobalSettings.Get().maxUnsupportedFeatureWarningCount = 0;
                    AssetDatabase.SaveAssets();
                }
#else
                errors = sb.ToString();
#endif
                unsupportedElements = null;
            }

        }

        public void WarnAboutUnsupportedFeature(string featureDescription, XmlReader reader = null)
        {
            if (GlobalSettings.Get().levelOfLog < LogLevel.ERRORS_AND_WARNINGS)
            {
                return;
            }

            //unsupportedFeatureWarningCount++;
            //if (unsupportedFeatureWarningCount > GlobalSettings.Get().maxUnsupportedFeatureWarningCount) {
            //    return;   
            //}

            if (reader != null)
            {
                // check if feature description has a namespace
                if (featureDescription.Contains(":"))
                {
                    //Debug.LogWarning("feature from namespace " + featureDescription.Substring(0, featureDescription.IndexOf(":")));
                    featureDescription = "namespaces are not supported. Using namespace " + featureDescription.Substring(0, featureDescription.IndexOf(":"));
                }
                //Add warning to the dictionary
                if (unsupportedElements == null)
                {
                    unsupportedElements = new Dictionary<string, List<int>>();
                }
                if (!unsupportedElements.ContainsKey(featureDescription))
                {
                    unsupportedElements.Add(featureDescription, new List<int>());
                }
                unsupportedElements[featureDescription].Add(((IXmlLineInfo)reader).LineNumber);
            }
            else
            {

#if UNITY_EDITOR
                if (!EditorUtility.DisplayDialog(
                    "Use of unsupported SVG feature detected!",
                    "Unsupported SVG features detected while importing the SVG document at:\n" + name + "\n\nRead the documentation and check your graphics production software's SVG exporter settings. Unless fixed, the graphic may not appear in Unity as intended.\n\nThe unsupported feature was:\n" + featureDescription + (reader != null ? " (at line " + ((IXmlLineInfo)reader).LineNumber + ")" : "") + (unsupportedFeatureWarningCount == GlobalSettings.Get().maxUnsupportedFeatureWarningCount ? "\n\nWarning limit reached. No more warnings will be given for this SVG document." : ""),
                    "Ok",
                    "Disable warnings globally")
                )
                {
                    GlobalSettings.Get().maxUnsupportedFeatureWarningCount = 0;
                    AssetDatabase.SaveAssets();
                }
#endif
            }
        }

        public bool Build()
        {
            if (document == null)
            {
                Debug.LogError("Document has not been imported");
                return false;
            }

            if (!BuildMeshFromDocument(this.document, out mesh, out collisionShapeData, true))
            {
                Debug.LogError("Failed to build a mesh from the document at " + name);
                return false;
            }
            this.meshLayers.Clear();
            this.collisionShapeDataLayers.Clear();
            if (this.importSettings.splitMeshesByLayers)
            {
                for (int i = 0; i < this.svgDocumentLayers.Count; ++i)
                {
                    Mesh m = null;
                    CollisionShapeData csd = null;
                    if (!BuildMeshFromDocument(this.svgDocumentLayers[i], out m, out csd, false))
                    {
                        Debug.LogError("Failed to build a mesh from the layer at " + name + " layer " + i);
                    }
                    this.meshLayers.Add(m);
                    this.collisionShapeDataLayers.Add(csd);
                    this.names.Add(this.svgDocumentLayers[i].GetRootID());
                }

            }
            return true;
        }

        public bool BuildMeshFromDocument(SVGDocument document, out Mesh mesh, out CollisionShapeData collisionData, bool moveByPivot)
        {
            mesh = null;
            collisionData = null;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> vertexColors = new List<Color>();

            // Start building
            bool documentTriangluatedSuccess = document.Triangulate(
                importSettings,
                ref vertices,
                ref triangles,
                ref vertexColors
            );

            if (!documentTriangluatedSuccess)
            {
                Debug.LogError("Triangulating the document failed");
                return false;
            }

            if (vertices.Count > 65534)
            {
                Debug.LogError("Triangulation produced a mesh with more than 65534 vertices. This is a limit imposed by Unity. Cannot continue. " + name);
                return false;

            }
            else if (vertices.Count < 3)
            {
                Debug.LogError("Less than 3 vertices were produced when triangulating the document. A mesh cannot be created.");
                return false;
            }

            // Calculate bounds
            Vector3 max = vertices[0];
            Vector3 min = vertices[0];
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];

                // Check and update bounds values
                min.x = Mathf.Min(min.x, vertex.x);
                min.y = Mathf.Min(min.y, vertex.y);
                min.z = Mathf.Min(min.z, vertex.z);

                max.x = Mathf.Max(max.x, vertex.x);
                max.y = Mathf.Max(max.y, vertex.y);
                max.z = Mathf.Max(max.z, vertex.z);
            }

            // Pivot shift
            Vector3 dim = new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), 0f);
            Vector3 pivotShift = new Vector3(
                min.x + importSettings.pivot.x * dim.x,
                max.y - importSettings.pivot.y * dim.y, // Use SVG coordinate system
                0f
            );

            Vector3 shiftedMin = min - pivotShift;
            Vector3 shiftedMax = max - pivotShift;

            List<Vector2> uv = new List<Vector2>(vertices.Count);

            for (int i = 0; i < vertices.Count; i++)
            {
                // UV
                uv.Add(new Vector2(
                    (vertices[i].x - min.x) / dim.x,
                    (vertices[i].y - min.y) / dim.y
                ));

                // Shift vertices by pivot offset
                if (moveByPivot)
                {
                    vertices[i] -= pivotShift;
                }

                // Apply uniform import scale
                vertices[i] *= importSettings.scale;
            }

            // Work around a bug in Unity's ParticleSystem
            Color32[] colors32 = new Color32[vertexColors.Count];
            for (int i = 0; i < colors32.Length; i++)
            {
                colors32[i] = vertexColors[i];
            }

            // Build regular mesh
            mesh = new Mesh();
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.colors32 = colors32; // Disable after Unity fixes their ParticleSystem
            mesh.SetUVs(0, uv);
#else
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors32 = colors32; // Disable after Unity fixes their ParticleSystem
            mesh.uv = uv.ToArray();
#endif

            if (moveByPivot)
            {
                mesh.bounds = new Bounds(
                    (shiftedMin + (shiftedMax - shiftedMin) / 2f) * importSettings.scale,
                    dim * importSettings.scale
                );
            }
            else
            {
                mesh.bounds = new Bounds(
                    (min + (max - min) / 2f) * importSettings.scale,
                    dim * importSettings.scale
                );
            }
            

            // Build collision shape
            List<Vector2> convexHull = ConvexHullUtility.QuickHull(vertices);
            collisionData = ScriptableObject.CreateInstance<CollisionShapeData>();
            collisionData.Add(convexHull);

            return true;
        }

#if UNITY_EDITOR
        public bool SaveAssets()
        {
            string svgPath = AssetDatabase.GetAssetPath(svgFile);
            string outputPathBase = svgPath.Substring(0, svgPath.Length - (svgPath.Length - svgPath.LastIndexOf('/'))) + "/";

            string mainAssetPath = outputPathBase + name + ".asset";

            // Check if there is already a asset container for this SVG file
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
            Object oldImportSettings = AssetDatabase.LoadAssetAtPath<ImportSettings>(mainAssetPath);
#else
            Object oldImportSettings = AssetDatabase.LoadAssetAtPath(mainAssetPath, typeof(ImportSettings));
#endif

            Object[] assetsAtPathCache = AssetDatabase.LoadAllAssetsAtPath(mainAssetPath);

            if (oldImportSettings == null)
            {
                AssetDatabase.CreateAsset(this.importSettings, mainAssetPath);
                AssetDatabase.SaveAssets();
            }

            SaveMesh(mainAssetPath, mesh, name + "-mesh", assetsAtPathCache);
            if (this.importSettings.splitMeshesByLayers)
            {
                for (int i = 0; i < this.meshLayers.Count; ++i)
                {
                    string nameOfLayer = name + "-mesh-layer-" + i;
                    if(names.Count > i && !string.IsNullOrEmpty(names[i]))
                    {
                        nameOfLayer += "-" + names[i];
                    }
                    SaveMesh(mainAssetPath, this.meshLayers[i], nameOfLayer, assetsAtPathCache);
                }
            }
            else
            {
                //remove layers of mesh
                for (int i = 0; i < assetsAtPathCache.Length; ++i)
                {
                    if (assetsAtPathCache[i] is Mesh)
                    {
                        if (assetsAtPathCache[i].name.Contains("-mesh-layer-"))
                        {
                            Object.DestroyImmediate(assetsAtPathCache[i], true);
                        }
                    }
                }
            }
            this.meshLayers.Clear();

            SaveCollision(mainAssetPath, collisionShapeData, name + "-collision", assetsAtPathCache);
            if (this.importSettings.SplitCollidersByLayers)
            {
                for (int i = 0; i < this.collisionShapeDataLayers.Count; ++i)
                {
                    string nameOfLayer = name + "-collision-layer-" + i;
                    if (names.Count > i && !string.IsNullOrEmpty(names[i]))
                    {
                        nameOfLayer += "-" + names[i];
                    }
                    SaveCollision(mainAssetPath, this.collisionShapeDataLayers[i], nameOfLayer, assetsAtPathCache);
                }
            }
            else
            {
                //remove layers of collisions
                for (int i = 0; i < assetsAtPathCache.Length; ++i)
                {
                    if (assetsAtPathCache[i] is CollisionShapeData)
                    {
                        if (assetsAtPathCache[i].name.Contains("-collision-layer-"))
                        {
                            Object.DestroyImmediate(assetsAtPathCache[i], true);
                        }
                    }
                }
            }
            this.collisionShapeDataLayers.Clear();

            // Make sure import settings carries the right name
            // This should have no effect if the ImportSettings object is not a subasset
#if !(UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
            ImportSettings settings = AssetDatabase.LoadAssetAtPath<ImportSettings>(mainAssetPath);
#else
            ImportSettings settings = AssetDatabase.LoadAssetAtPath(mainAssetPath, typeof(ImportSettings)) as ImportSettings;
#endif
            settings.name = name + "-settings";
            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(mainAssetPath);
            return true;
        }

        private void SaveMesh(string mainAssetPath, Mesh mesh, string meshName, Object[] assetsAtPathCache)
        {
            if (mesh != null)
            {
                mesh.name = meshName;

                // Preserve the old container an only replace changed assets

                Mesh mainMesh = null;
                for (int i = 0; i < assetsAtPathCache.Length; ++i)
                {
                    if (assetsAtPathCache[i] is Mesh)
                    {
                        mainMesh = assetsAtPathCache[i] as Mesh;
                        if (mainMesh != null && mainMesh.name == meshName)
                        {
                            break;
                        }
                        mainMesh = null;
                    }
                }

                if (mainMesh == null)
                {
                    AssetDatabase.AddObjectToAsset(mesh, mainAssetPath);
                }
                else
                {
                    // Replace mesh data
                    mainMesh.Clear();

                    mainMesh.name = mesh.name;
                    mainMesh.vertices = mesh.vertices;
                    mainMesh.colors = null; // Remove old colors from meshes if present. Otherwise they just confuse the buggy ParticleSystem
                    mainMesh.colors32 = mesh.colors32;
                    mainMesh.triangles = mesh.triangles;
                    mainMesh.normals = mesh.normals;
                    mainMesh.uv = mesh.uv;

                    mainMesh.RecalculateBounds();

                    EditorUtility.SetDirty(mainMesh);
                    AssetDatabase.SaveAssets();

                    Object.DestroyImmediate(mesh);
                }
            }
            else if (GlobalSettings.Get().levelOfLog == LogLevel.ERRORS_AND_WARNINGS)
            {
                Debug.LogWarning("Mesh missing. Cannot save mesh asset.");
            }
        }

        private void SaveCollision(string mainAssetPath, CollisionShapeData collisionData, string collisionName, Object[] assetsAtPathCache)
        {

            if (collisionData != null)
            {
                collisionData.name = collisionName;

                CollisionShapeData collisionShapeAsset = null;
                for (int i = 0; i < assetsAtPathCache.Length; ++i)
                {
                    if (assetsAtPathCache[i] is CollisionShapeData)
                    {
                        collisionShapeAsset = assetsAtPathCache[i] as CollisionShapeData;
                        if (collisionShapeAsset != null && collisionShapeAsset.name == collisionName)
                        {
                            break;
                        }
                        collisionShapeAsset = null;
                    }
                }

                if (collisionShapeAsset == null)
                {
                    AssetDatabase.AddObjectToAsset(collisionData, mainAssetPath);
                }
                else
                {
                    collisionShapeAsset.Clear();
                    collisionShapeAsset.name = collisionData.name;
                    collisionShapeAsset.collisionPolygons = collisionData.collisionPolygons;

                    EditorUtility.SetDirty(collisionShapeAsset);
                    AssetDatabase.SaveAssets();

                    Object.DestroyImmediate(collisionData);
                    collisionData = null;
                }
            }
            else if (GlobalSettings.Get().levelOfLog == LogLevel.ERRORS_AND_WARNINGS)
            {
                Debug.LogWarning("Collision data missing. Cannot save collision asset.");
            }
        }
#endif
    }
}
