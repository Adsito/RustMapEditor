using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainTransparency))]
public class TerrainTransparencyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainTransparency script = (TerrainTransparency)target;
        GUILayout.Label("Transparency");
        script.alpha = float.Parse(GUILayout.TextField(script.alpha + ""));
        script.alpha = GUILayout.HorizontalSlider(script.alpha, 0.0f, 1);
        
        if (script.alpha != script.oldValue)
        {
            Material mat = script.GetComponent<Terrain>().materialTemplate;
            Color c = mat.color; 
            c.a = script.alpha;
            mat.color = c;

            script.oldValue = script.alpha;
        }
    }
}
