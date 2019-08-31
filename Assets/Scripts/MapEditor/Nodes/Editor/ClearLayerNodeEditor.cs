using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(ClearLayerNode))]
public class ClearLayerNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Clear Layer", "Clears the selected layer's textures."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        ClearLayerNode node = target as ClearLayerNode;
        if (node.layer == NodeVariables.Misc.DualLayerEnum.Topology)
        {
            node.topologies = (TerrainTopologySDK.Enum)EditorGUILayout.EnumFlagsField(node.topologies);
        }
    }
}