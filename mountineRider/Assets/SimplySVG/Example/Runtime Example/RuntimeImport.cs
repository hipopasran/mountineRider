// Copyright © 2015 NordicEdu Ltd.

using UnityEngine;
using SimplySVG;


public class RuntimeImport : MonoBehaviour {

    public string svgData;
    public MeshFilter meshFilter;
    
    void OnGUI()
    {
        if (GUI.Button(new Rect(10,10,200,20), "import SVG"))
        {
            SimplySVGImporter importer = new SimplySVGImporter(svgData, "test object");
            importer.Import();
            importer.Build();
            //Results
            if (string.IsNullOrEmpty(importer.errors))
            {
                meshFilter.mesh = importer.mesh;
            }
            else
            {
                Debug.Log(importer.errors);
            }
        }
        svgData = GUI.TextArea(new Rect(10, 40, 200, 500), svgData);
    }    
}
