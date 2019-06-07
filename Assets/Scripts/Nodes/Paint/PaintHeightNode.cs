using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Paint Height")]
public class PaintHeightNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output] public NodeVariables.NextTask NextTask;
    [NonSerialized()] public float heightLow = 0f, heightHigh = 500f, heightMinBlendLow = 0f, heightMaxBlendLow = 500f, heightMinBlendHigh = 500f, heightMaxBlendHigh = 1000f;
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
                mapIO.changeLayer("Ground");
                mapIO.PaintHeight("Ground", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                mapIO.changeLayer("Biome");
                mapIO.PaintHeight("Biome", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                mapIO.changeLayer("Alpha");
                mapIO.PaintHeight("Alpha", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, layer.AlphaTexture);
                break;
            case 3: // Topology. Going to overhaul the topology layers soon to avoid all the changing of layer values.
                mapIO.changeLayer("Topology");
                mapIO.oldTopologyLayer2 = mapIO.topologyLayer;

                mapIO.topologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(layer.TopologyLayer);
                mapIO.changeLandLayer();
                mapIO.oldTopologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(layer.TopologyLayer);
                mapIO.PaintHeight("Topology", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, layer.TopologyTexture);

                mapIO.topologyLayer = mapIO.oldTopologyLayer2;
                mapIO.changeLandLayer();
                mapIO.oldTopologyLayer = mapIO.oldTopologyLayer2;
                break;
        }
    }
}
