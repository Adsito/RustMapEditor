using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;
using NodeVariables;

[CustomNodeEditor(typeof(TextureNode))]
public class TextureNodeEditor : NodeEditor
{
    #region Fields
    TerrainSplat.Enum groundEnum = TerrainSplat.Enum.Grass;
    TerrainBiome.Enum biomeEnum = TerrainBiome.Enum.Temperate;
    NodeVariables.Texture.AlphaEnum alphaEnum = NodeVariables.Texture.AlphaEnum.Active;
    TerrainTopology.Enum topologyLayer = TerrainTopology.Enum.Beach;
    NodeVariables.Texture.TopologyEnum topologyEnum = NodeVariables.Texture.TopologyEnum.Active;
    #endregion
    public override Color GetTint()
    {
        return Color.green;   
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Texture", "Select the texture from this node, and input it into any accepting nodes to choose the texture to paint."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        TextureNode node = target as TextureNode;
        AutoGenerationGraph graph = node.graph as AutoGenerationGraph;
        switch (node.landLayer)
        {
            case NodeVariables.Texture.LandLayerEnum.Ground:
                groundEnum = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture", groundEnum);
                break;
            case NodeVariables.Texture.LandLayerEnum.Biome:
                biomeEnum = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture", biomeEnum);
                break;
            case NodeVariables.Texture.LandLayerEnum.Alpha:
                alphaEnum = (NodeVariables.Texture.AlphaEnum)EditorGUILayout.EnumPopup("Texture", alphaEnum);
                break;
            case NodeVariables.Texture.LandLayerEnum.Topology:
                topologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology", topologyLayer);
                topologyEnum = (NodeVariables.Texture.TopologyEnum)EditorGUILayout.EnumPopup("Texture", topologyEnum);
                break;
        }
        node.Texture.LandLayer = (int)node.landLayer;
        node.Texture.TopologyLayer = TerrainTopology.TypeToIndex((int)topologyLayer);
        node.Texture.GroundTexture = (int)groundEnum;
        node.Texture.BiomeTexture = (int)biomeEnum;
        node.Texture.AlphaTexture = (int)alphaEnum;
        node.Texture.TopologyTexture = (int)topologyEnum;
    }
}
