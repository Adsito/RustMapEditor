using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(SmoothHeightMapNode))]
public class SmoothHeightMapNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Smooth HeightMap", "Smoothes the heightmap."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        SmoothHeightMapNode node = target as SmoothHeightMapNode;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Strength", "The strength of the smoothing operation."), GUILayout.MaxWidth(50));
        node.filterStrength = EditorGUILayout.Slider(node.filterStrength, 0f, 1f);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Blur", "The direction the terrain should blur towards. Negative is down, " +
            "positive is up."), GUILayout.MaxWidth(50));
        node.blurDirection = EditorGUILayout.Slider(node.blurDirection, -1f, 1f);
        EditorGUILayout.EndHorizontal();
    }
}