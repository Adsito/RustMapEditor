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
    TerrainSplat.Enum groundEnum;
    TerrainBiome.Enum biomeEnum;
    NodeVariables.Texture.AlphaEnum alphaEnum;
    TerrainTopology.Enum topologyLayer;
    NodeVariables.Texture.TopologyEnum topologyEnum;
    #endregion
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
        node.Out.LandLayer = (int)node.landLayer;
        node.Out.TopologyLayer = TerrainTopology.TypeToIndex((int)topologyLayer);
        node.Out.GroundTexture = (int)groundEnum;
        node.Out.BiomeTexture = (int)biomeEnum;
        node.Out.AlphaTexture = (int)alphaEnum;
        node.Out.TopologyTexture = (int)topologyEnum;
    }
}
