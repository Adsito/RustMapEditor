using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(CopyTextureNode))]
public class CopyTextureNodeEditor : NodeEditor
{
    int landLayerFrom = 0;
    int[] values = { 0, 1 };
    string[] activeTextureAlpha = { "Visible", "Invisible" };
    string[] activeTextureTopo = { "Active", "Inactive" };
    TerrainSplat.Enum groundLayerFrom = TerrainSplat.Enum.Grass;
    TerrainBiome.Enum biomeLayerFrom = TerrainBiome.Enum.Temperate;
    TerrainTopology.Enum topologyLayerFrom = TerrainTopology.Enum.Beach;
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
        landLayerFrom = EditorGUILayout.Popup("Layer:", landLayerFrom, node.landLayers);
        switch (landLayerFrom) // Get texture list from the currently selected landLayer.
        {
            case 0:
                groundLayerFrom = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture:", groundLayerFrom);
                node.textureFrom = TerrainSplat.TypeToIndex((int)groundLayerFrom);
                break;
            case 1:
                biomeLayerFrom = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture:", biomeLayerFrom);
                node.textureFrom = TerrainBiome.TypeToIndex((int)biomeLayerFrom);
                break;
            case 2:
                node.textureFrom = EditorGUILayout.IntPopup("Texture:", node.textureFrom, activeTextureAlpha, values);
                break;
            case 3:
                topologyLayerFrom = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology:", topologyLayerFrom);
                node.topologyFrom = TerrainTopology.TypeToIndex((int)topologyLayerFrom);
                node.textureFrom = EditorGUILayout.IntPopup("Texture:", node.textureFrom, activeTextureTopo, values);
                break;
        }
    }
}
