using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(CopyTextureNode))]
public class CopyTextureNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.magenta;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Copy Texture", "Copies the texture selected below and paints it with the texture inputted into the node."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        CopyTextureNode node = target as CopyTextureNode;
        NodeVariables.Texture texture = (NodeVariables.Texture)node.GetValue();
        GUILayout.Label("Copy Textures", EditorStyles.boldLabel);
        node.landLayerFrom = EditorGUILayout.Popup("Layer:", node.landLayerFrom, node.landLayers);
        switch (node.landLayerFrom) // Get texture list from the currently selected landLayer.
        {
            case 0:
                node.groundLayerFrom = (TerrainSplatSDK.Enum)EditorGUILayout.EnumPopup("Texture:", node.groundLayerFrom);
                node.textureFrom = TerrainSplatSDK.TypeToIndex((int)node.groundLayerFrom);
                break;
            case 1:
                node.biomeLayerFrom = (TerrainBiomeSDK.Enum)EditorGUILayout.EnumPopup("Texture:", node.biomeLayerFrom);
                node.textureFrom = TerrainBiomeSDK.TypeToIndex((int)node.biomeLayerFrom);
                break;
            case 2:
                node.textureFrom = EditorGUILayout.IntPopup("Texture:", node.textureFrom, node.activeTextureAlpha, node.values);
                break;
            case 3:
                node.topologyLayerFrom = (TerrainTopologySDK.Enum)EditorGUILayout.EnumPopup("Topology:", node.topologyLayerFrom);
                node.topologyFrom = TerrainTopologySDK.TypeToIndex((int)node.topologyLayerFrom);
                node.textureFrom = EditorGUILayout.IntPopup("Texture:", node.textureFrom, node.activeTextureTopo, node.values);
                break;
        }
    }
}
