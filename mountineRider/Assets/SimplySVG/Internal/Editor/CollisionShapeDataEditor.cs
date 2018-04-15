// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimplySVG {
	[CustomEditor(typeof(CollisionShapeData))]
	public class CollisionShapeDataEditor : Editor {
		//CollisionShapeData collisionShapeData;
		
		void OnEnable() {
			//collisionShapeData = (CollisionShapeData)target;
		}
		
		override public void OnInspectorGUI() {
			EditorGUILayout.HelpBox ("This object generates a collider for a SVG object.\n\nHere is how:\n1. Add a ColliderHelper component to the target GameObject\n2. Drag and Drop this asset to the SVG collision property field\n3. Press \"Update Collider\"", MessageType.Info);
		}
	}
}
