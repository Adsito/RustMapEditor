using UnityEngine;
using XNode;

[CreateNodeMenu("Texture")]
public class TextureNode : Node
{
    [Output] public NodeVariables.Texture Texture;
    #region Fields
    public NodeVariables.Texture.LandLayerEnum landLayer;
    [HideInInspector] public TerrainSplat.Enum groundEnum = TerrainSplat.Enum.Grass;
    [HideInInspector] public TerrainBiome.Enum biomeEnum = TerrainBiome.Enum.Temperate;
    [HideInInspector] public NodeVariables.Texture.AlphaEnum alphaEnum;
    [HideInInspector] public TerrainTopology.Enum topologyLayer = TerrainTopology.Enum.Beach;
    [HideInInspector] public NodeVariables.Texture.TopologyEnum topologyEnum;
    #endregion
    public override object GetValue(NodePort port)
    {
        return Texture;
    }
}