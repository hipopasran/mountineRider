// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SimplySVG {
    public class CreateMeshRendererMenuItem {
        [MenuItem("GameObject/2D Object/Simply SVG Mesh Renderer")]
        public static void CreateMeshRendererMenuEntry() {
            GameObject go = new GameObject("New Simply SVG Renderer");
            Undo.RegisterCreatedObjectUndo(go, "New Simply SVG Renderer");

            go.AddComponent<MeshFilter>();

            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.material = GlobalSettings.Get().defaultMaterial;

            Selection.activeObject = go;
        }
    }
}
