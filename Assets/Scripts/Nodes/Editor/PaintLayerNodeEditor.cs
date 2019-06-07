using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(PaintLayerNode))]
public class PaintLayerNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.magenta;
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PaintLayerNode node = target as PaintLayerNode;
    }
}
