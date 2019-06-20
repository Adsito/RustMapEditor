using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Copy Texture")]
public class CopyTextureNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask NextTask;
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
        MapIO mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
        if (layer == null) // Check for if the textures node is not connected.
        {
            return;
        }
        switch (layer.LandLayer)
        {
            case 0: // Ground
                mapIO.CopyTexture(landLayers[landLayerFrom], "Ground", textureFrom, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                mapIO.CopyTexture(landLayers[landLayerFrom], "Biome", textureFrom, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                mapIO.CopyTexture(landLayers[landLayerFrom], "Alpha", textureFrom, layer.AlphaTexture);
                break;
            case 3: // Topology.
                mapIO.CopyTexture(landLayers[landLayerFrom], "Topology", textureFrom, layer.TopologyTexture, topologyFrom, layer.TopologyLayer);
                break;
        }
    }
}
