using UnityEngine;
using XNode;

[CreateNodeMenu("Texture")]
public class TextureNode : Node
{
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    #region Fields
    [NodeEnum] public NodeVariables.Texture.LandLayerEnum landLayer = NodeVariables.Texture.LandLayerEnum.Ground;
    [HideInInspector] public TerrainSplatSDK.Enum groundEnum = TerrainSplatSDK.Enum.Grass;
    [HideInInspector] public TerrainBiomeSDK.Enum biomeEnum = TerrainBiomeSDK.Enum.Temperate;
    [HideInInspector] public NodeVariables.Texture.AlphaEnum alphaEnum = NodeVariables.Texture.AlphaEnum.Active;
    [HideInInspector] public TerrainTopologySDK.Enum topologyLayer = TerrainTopologySDK.Enum.Beach;
    [HideInInspector] public NodeVariables.Texture.TopologyEnum topologyEnum = NodeVariables.Texture.TopologyEnum.Active;
    #endregion
    public override object GetValue(NodePort port)
    {
        return Texture;
    }
}