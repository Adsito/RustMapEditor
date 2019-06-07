using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(StartNode))]
public class StartNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.red;
    }
}
