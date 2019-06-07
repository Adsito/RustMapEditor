using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(StartNode))]
public class StartNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.red;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Start", "The starting point of the nodes in the graph. Only nodes connected to this node directly or through other nodes will be applied."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
}
