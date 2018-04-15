// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace SimplySVG {
    public class RenderSorter : MonoBehaviour
    {
        public bool autoUpdate = false;
        public int sortingLayerID;

        public void Sort() {
            int orderCounter = 0;
            UpdateChildren(transform, ref orderCounter);
        }

        void Update() {
            if (autoUpdate) {
                Sort();
            }
        }

        void UpdateChildren(Transform node, ref int orderCounter) {

            Renderer component = node.GetComponent<Renderer>();
            RendererProperties renderProperties = node.GetComponent<RendererProperties>();
            if (renderProperties != null)
            {
                if (renderProperties.layerId == sortingLayerID)
                {
                    renderProperties.SetRenderOrder(orderCounter);
                    orderCounter++;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(renderProperties);
#endif
                }
            }
            else if (component != null) {
                if (component.sortingLayerID == sortingLayerID) {
                    component.sortingOrder = orderCounter;
                    orderCounter++;
#if UNITY_EDITOR
                    EditorUtility.SetDirty(component);
#endif
                }
            }
            
            for (int i = 0; i < node.childCount; i++) {
                Transform child = node.GetChild(i);
                UpdateChildren(child, ref orderCounter);
            }
        }
    }
}
