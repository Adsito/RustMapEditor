using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(NormaliseHeightMapNode))]
public class NormaliseHeightMapNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Normalise HeightMap", "Normalise the heightmap between 2 heights."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        NormaliseHeightMapNode node = target as NormaliseHeightMapNode;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Low", "The lowest point on the map after being normalised."), GUILayout.MaxWidth(40));
        EditorGUI.BeginChangeCheck();
        node.normaliseLow = EditorGUILayout.Slider(node.normaliseLow, 0f, 1000f);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("High", "The highest point on the map after being normalised."), GUILayout.MaxWidth(40));
        node.normaliseHigh = EditorGUILayout.Slider(node.normaliseHigh, 0f, 1000f);
        EditorGUILayout.EndHorizontal();
    }
}