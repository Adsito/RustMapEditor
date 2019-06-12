using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(InvertLayerNode))]
public class InvertLayerNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Invert Layer", "Inverts the selected layer's textures."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
}