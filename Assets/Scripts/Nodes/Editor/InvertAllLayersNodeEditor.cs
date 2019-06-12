using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(InvertAllLayersNode))]
public class InvertAllLayersNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Invert All Layers", "Inverts all the textures on the landlayer selected."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
}