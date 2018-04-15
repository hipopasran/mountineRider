// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimplySVG {
    [CustomEditor(typeof(ColliderHelper))]
    public class ColliderHelperEditor : Editor {
        static bool showHelp = false;

        ColliderHelper assigner;

        void OnEnable() {
            assigner = (ColliderHelper)target;
        }

        override public void OnInspectorGUI() {
            Undo.RecordObject(assigner, "Collider Helper");

            showHelp = EditorGUILayout.Foldout(showHelp, "Help");
            if (showHelp) {
                EditorGUILayout.HelpBox("Use this helper to assign the generated collider data to this GameObject's PolygonCollider2D component.", MessageType.Info);
                GUILayout.Space(8f);
            }

            CollisionShapeData newShapeData = (CollisionShapeData)EditorGUILayout.ObjectField("SVG Collision Data", assigner.collisionShapeData, typeof(CollisionShapeData), false);
            if (newShapeData != null && newShapeData != assigner.collisionShapeData) {
                assigner.collisionShapeData = newShapeData;
                assigner.UpdateColliderShape();
            }

            assigner.autoUpdateOnAwake = EditorGUILayout.Toggle(new GUIContent("Update On Awake", "Automatically update the collider on awake.\n\nUpon SVG reimport, the collider shape may change but the PolygonCollider2D does not update automatically. Turn this option on if you want to be sure the latest collider version is always used. Otherwise you will need to manually press the \"Update collider\" button."), assigner.autoUpdateOnAwake);

            if (GUILayout.Button("Update collider")) {
                assigner.UpdateColliderShape();
            }
        }
    }
}
