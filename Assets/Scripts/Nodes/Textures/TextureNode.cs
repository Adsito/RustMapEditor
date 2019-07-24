using UnityEngine;
using XNode;

[CreateNodeMenu("Texture")]
public class TextureNode : Node
{
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    #region Fields
    public NodeVariables.Texture.LandLayerEnum landLayer = NodeVariables.Texture.LandLayerEnum.Ground;
    [HideInInspector] public TerrainSplat.Enum groundEnum = TerrainSplat.Enum.Grass;
    [HideInInspector] public TerrainBiome.Enum biomeEnum = TerrainBiome.Enum.Temperate;
    [HideInInspector] public NodeVariables.Texture.AlphaEnum alphaEnum = NodeVariables.Texture.AlphaEnum.Active;
    [HideInInspector] public TerrainTopology.Enum topologyLayer = TerrainTopology.Enum.Beach;
    [HideInInspector] public NodeVariables.Texture.TopologyEnum topologyEnum = NodeVariables.Texture.TopologyEnum.Active;
    #endregion
    public override object GetValue(NodePort port)
    {
        return Texture;
    }
}