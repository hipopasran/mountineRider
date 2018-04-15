// Copyright © 2015 NordicEdu Ltd.

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace SimplySVG {
    [InitializeOnLoad]
    public class DnDToScene : Editor {

        private static Vector3 customPos;
        private static Quaternion customRot;

        static DnDToScene()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;
            SceneView.onSceneGUIDelegate += OnSeceneGUI;
        }

        static void OnSeceneGUI(SceneView sceneView)
        {
            //Set position!

            Vector3 mousepos = Event.current.mousePosition;
            mousepos.y = sceneView.camera.pixelHeight - mousepos.y;
            customPos = sceneView.camera.ScreenToWorldPoint(mousepos);

            if (sceneView.in2DMode) {
                customPos.z = 0;
                customRot = Quaternion.identity;

            } else {
                customPos += sceneView.camera.transform.forward * 5f;

                customRot = Quaternion.LookRotation(
                    sceneView.camera.transform.forward,
                    sceneView.camera.transform.up
                );
            }

            UpdateDragging();
        }
    
        static void HierarchyWindowItemCallback(int pID, Rect pRect)
        {
            customPos = Vector3.zero;
            UpdateDragging();
        }

        private static void UpdateDragging() {
            if (Event.current.type == EventType.DragPerform || Event.current.type == EventType.DragUpdated) {
                if (DragAndDrop.objectReferences.Length < 1) {
                    // A file from outside of the project was D&D'd to the scene.
                    return;

                } else if (!EditorUtilities.CheckAssetFileTypeByExtension(DragAndDrop.objectReferences[0], ".svg")) {
                    return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (Event.current.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();

                    List<GameObject> selectedObjects = EditorUtilities.MakeGameObjectsForSVGAssets(DragAndDrop.objectReferences);

                    foreach (GameObject go in selectedObjects) {
                        go.transform.position = customPos;
                        go.transform.rotation = customRot;
                    }

                    if (selectedObjects != null && selectedObjects.Count > 0) {
                        Selection.objects = selectedObjects.ToArray();
                    }
                }

                Event.current.Use();
            }
        }
    }
}
