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
}