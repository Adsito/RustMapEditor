using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(InvertHeightMapNode))]
public class InvertHeightMapNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Invert HeightMap", "Inverts the heightmap."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
}