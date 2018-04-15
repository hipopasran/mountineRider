// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections;


namespace SimplySVG
{
    //[RequireComponent(typeof(Renderer))]
    [ExecuteInEditMode]
    public class RendererProperties : MonoBehaviour
    {

        Renderer render;

        public int layerId = int.MaxValue;
        public int order = int.MaxValue;

        public void SetRenderLayer(int layerId)
        {
            this.layerId = layerId;
            GetTargetRenderer().sortingLayerID = layerId;
        }

        public void SetRenderOrder(int order)
        {
            this.order = order;
            GetTargetRenderer().sortingOrder = order;
        }

        public void Save()
        {
            if (this.layerId != int.MaxValue)
            {
                GetTargetRenderer().sortingLayerID = this.layerId;
            }
            if (this.order != int.MaxValue)
            {
                GetTargetRenderer().sortingOrder = this.order;
            }
        }

        public void OnEnable()
        {
            if (this.layerId != int.MaxValue)
            {
                SetRenderLayer(this.layerId);
            }
            if (this.order != int.MaxValue)
            {
                SetRenderOrder(this.order);
            }
        }

        public Renderer GetTargetRenderer()
        {
            if (this.render == null)
            {
                this.render = GetComponent<Renderer>();
            }
            if (Application.isEditor && this.render == null)
            {
                Debug.LogError("Simply SVG renderer properties should only be within game objects that have a renderer!");
            }

            return this.render;
        }
    }
}
