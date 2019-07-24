using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(RotateLayerNode))]
public class RotateLayerNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Rotate Layer", "Rotates the layer of the inputted texture."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        RotateLayerNode node = target as RotateLayerNode;
        if (node.landLayer == NodeVariables.Texture.LandLayerEnum.Topology)
        {
            node.topologies = (TerrainTopology.Enum)EditorGUILayout.EnumFlagsField(node.topologies);
        }
    }
}