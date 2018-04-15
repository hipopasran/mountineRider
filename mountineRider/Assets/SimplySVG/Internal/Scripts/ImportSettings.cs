// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using System.Collections;

public class ImportSettings : ScriptableObject {
    public Object svgFile = null;

    public bool splitMeshesByLayers = false;
    public bool _splitCollidersByLayers = true;
    public bool SplitCollidersByLayers
    {
        get
        {
            return splitMeshesByLayers && _splitCollidersByLayers;
        }
    }
    public float scale = 0.01f;
    public int maxSubdivisonDepth = 8;
    public float minSubdivisionDistanceDelta = 0.3333333f;
    public Vector2 pivot = new Vector2(0.5f, 0.5f);
}
