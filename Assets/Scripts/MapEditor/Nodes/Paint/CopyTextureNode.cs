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
    [HideInInspector] public TerrainSplat.Enum groundLayerFrom = TerrainSplat.Enum.Grass;
    [HideInInspector] public TerrainBiome.Enum biomeLayerFrom = TerrainBiome.Enum.Temperate;
    [HideInInspector] public TerrainTopology.Enum topologyLayerFrom = TerrainTopology.Enum.Beach;
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
            case 0: 
                MapIO.CopyTexture(landLayers[landLayerFrom], "Ground", textureFrom, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: 
                MapIO.CopyTexture(landLayers[landLayerFrom], "Biome", textureFrom, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2:
                MapIO.CopyTexture(landLayers[landLayerFrom], "Alpha", textureFrom, layer.AlphaTexture);
                break;
            case 3: 
                MapIO.CopyTexture(landLayers[landLayerFrom], "Topology", textureFrom, layer.TopologyTexture, topologyFrom, layer.TopologyLayer);
                break;
        }
    }
}