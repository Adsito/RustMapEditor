using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Copy Texture")]
public class CopyTextureNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    #region Fields
    [HideInInspector] public int landLayerFrom, textureFrom, topologyFrom;
    [HideInInspector] public string[] landLayers = { "Ground", "Biome", "Alpha", "Topology" };
    [HideInInspector] public int[] values = { 0, 1 };
    [HideInInspector] public string[] activeTextureAlpha = { "Visible", "Invisible" };
    [HideInInspector] public string[] activeTextureTopo = { "Active", "Inactive" };
    [HideInInspector] public TerrainSplatSDK.Enum groundLayerFrom = TerrainSplatSDK.Enum.Grass;
    [HideInInspector] public TerrainBiomeSDK.Enum biomeLayerFrom = TerrainBiomeSDK.Enum.Temperate;
    [HideInInspector] public TerrainTopologySDK.Enum topologyLayerFrom = TerrainTopologySDK.Enum.Beach;
    #endregion
    public override object GetValue(NodePort port)
    {
        NodeVariables.Texture Texture = GetInputValue("Texture", this.Texture);
        return Texture;
    }
    public object GetValue()
    {
        return GetInputValue<object>("Texture");
    }
    public void RunNode()
    {
        var layer = (NodeVariables.Texture)GetValue();
        if (layer == null) // Check for if the textures node is not connected.
        {
            return;
        }
        switch (layer.LandLayer)
        {
            case 0: // Ground
                MapIO.CopyTexture(landLayers[landLayerFrom], "Ground", textureFrom, TerrainSplatSDK.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                MapIO.CopyTexture(landLayers[landLayerFrom], "Biome", textureFrom, TerrainBiomeSDK.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                MapIO.CopyTexture(landLayers[landLayerFrom], "Alpha", textureFrom, layer.AlphaTexture);
                break;
            case 3: // Topology.
                MapIO.CopyTexture(landLayers[landLayerFrom], "Topology", textureFrom, layer.TopologyTexture, topologyFrom, layer.TopologyLayer);
                break;
        }
    }
}