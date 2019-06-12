using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(ClearAllLayersNode))]
public class ClearAllLayersNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Clear All Layers", "Clears all the textures on the landlayer selected."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
}