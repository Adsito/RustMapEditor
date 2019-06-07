using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(PaintLayerNode))]
public class PaintLayerNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.magenta;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Paint Layer", "Paints the texture over the entire layer."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PaintLayerNode node = target as PaintLayerNode;
    }
}
