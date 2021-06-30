using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathDataHolder))]
public class PathDataHolderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PathDataHolder script = (PathDataHolder)target;
        script.pathData.name = EditorGUILayout.TextField("Name", script.pathData.name + "");
        script.pathData.spline = EditorGUILayout.Toggle("Spline", script.pathData.spline);
        script.pathData.start = EditorGUILayout.Toggle("Start", script.pathData.start);
        script.pathData.end = EditorGUILayout.Toggle("End", script.pathData.end);
        script.pathData.width = EditorGUILayout.FloatField("Width", script.pathData.width);

        script.pathData.innerPadding = EditorGUILayout.FloatField("Inner Padding", script.pathData.innerPadding);
        script.pathData.outerPadding = EditorGUILayout.FloatField("Outer Padding", script.pathData.outerPadding);

        script.pathData.innerFade = EditorGUILayout.FloatField("Inner Fade", script.pathData.innerFade);
        script.pathData.outerFade = EditorGUILayout.FloatField("Outer Fade", script.pathData.outerFade);
        
        script.pathData.randomScale = EditorGUILayout.FloatField("Random Scale", script.pathData.randomScale);
        script.pathData.meshOffset = EditorGUILayout.FloatField("Mesh Offset", script.pathData.meshOffset);
        script.pathData.terrainOffset = EditorGUILayout.FloatField("Terrain Offset", script.pathData.terrainOffset);

        script.pathData.splat = EditorGUILayout.IntField("Splat", script.pathData.splat);
        script.pathData.topology = EditorGUILayout.IntField("Topology", script.pathData.topology);

        GUILayout.Label("Path Tools", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add path node to start"))
            script.AddNodeToStart();
        if (GUILayout.Button("Add path node to end"))
            script.AddNodeToEnd();
        GUILayout.EndHorizontal();
        GUILayout.Label("Node Resolution Factor");
        script.resolutionFactor = float.Parse(GUILayout.TextField(script.resolutionFactor + ""));
        script.resolutionFactor = GUILayout.HorizontalSlider(script.resolutionFactor, 0, 1);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Increase Nodes Resolution"))
            script.IncreaseNodesRes();
        if (GUILayout.Button("Decrease Nodes Resolution"))
            script.DecreaseNodesRes();
        GUILayout.EndHorizontal();
    }
}
