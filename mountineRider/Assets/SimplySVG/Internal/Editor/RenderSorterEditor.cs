// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace SimplySVG {
    [CustomEditor(typeof(RenderSorter))]
    public class RenderSorterEditor : Editor {
        bool showHelp = false;

        RenderSorter renderSorter;

        void OnEnable() {
            renderSorter = (RenderSorter)target;
        }

        override public void OnInspectorGUI() {
            Undo.RecordObject(renderSorter, "Render Sorter");

            showHelp = EditorGUILayout.Foldout(showHelp, "Help");
            if (showHelp) {
                EditorGUILayout.HelpBox("Set the rendering order of child Renderers to match the scene hierarchy order.", MessageType.Info);
            }

            renderSorter.sortingLayerID = EditorUtilities.SortingLayerField(new GUIContent("Affect sorting layer", "This sorter will only sort child Renderers with a matching layer"), serializedObject.FindProperty("sortingLayerID"));

            bool newAutoUpdate = EditorGUILayout.Toggle(new GUIContent("Sort automatically", "Re-sort the Renderers in the child hierarchy for each frame"), renderSorter.autoUpdate);
            if (newAutoUpdate != renderSorter.autoUpdate && newAutoUpdate) {
                SortAndRepaintEditor();
            }
            renderSorter.autoUpdate = newAutoUpdate;

            if (GUILayout.Button("Sort")) {
                SortAndRepaintEditor();
            }
        }

        void OnSceneGUI() {
            if (renderSorter.autoUpdate) {
                renderSorter.Sort();
            }
        }

        void SortAndRepaintEditor() {
            renderSorter.Sort();
            SceneView.RepaintAll();
        }
    }
}
