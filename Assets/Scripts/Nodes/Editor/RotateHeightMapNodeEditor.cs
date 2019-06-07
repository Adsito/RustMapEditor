using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(RotateHeightMapNode))]
public class RotateHeightMapNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Rotate HeightMap", "Rotates the heightmap and watermap."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        RotateHeightMapNode node = target as RotateHeightMapNode;
        node.CW = EditorGUILayout.ToggleLeft(new GUIContent(node.CW ? "Rotate Direction: 90°" : "Rotate Direction: 270°", "The direction which the heightmap and watermap will rotate, either 90° or 270°"), node.CW);
    }
}