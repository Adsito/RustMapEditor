using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

[CreateNodeMenu("Paint/Paint Slope")]
public class PaintSlopeNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture In;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output] public NodeVariables.NextTask NextTask;
    [NonSerialized()] public float slopeLow = 40f, slopeHigh = 60f, slopeMinBlendLow = 25f, slopeMaxBlendLow = 40f, slopeMinBlendHigh = 60f, slopeMaxBlendHigh = 75f;
    public override object GetValue(NodePort port)
    {
        NodeVariables.Texture In = GetInputValue("In", this.In);
        return In;
    }
    public object GetValue()
    {
        return GetInputValue<object>("In");
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
                mapIO.PaintSlope("Ground", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                mapIO.changeLayer("Biome");
                mapIO.PaintSlope("Biome", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainBiome.TypeToIndex(layer.GroundTexture));
                break;
            case 2: // Alpha
                mapIO.changeLayer("Alpha");
                mapIO.PaintSlope("Alpha", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, layer.AlphaTexture);
                break;
            case 3: // Topology. Going to overhaul the topology layers soon to avoid all the changing of layer values.
                mapIO.changeLayer("Topology");
                mapIO.oldTopologyLayer2 = mapIO.topologyLayer;

                mapIO.topologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(layer.TopologyLayer);
                mapIO.changeLandLayer();
                mapIO.oldTopologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(layer.TopologyLayer);
                mapIO.PaintSlope("Alpha", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, layer.TopologyTexture);

                mapIO.topologyLayer = mapIO.oldTopologyLayer2;
                mapIO.changeLandLayer();
                mapIO.oldTopologyLayer = mapIO.oldTopologyLayer2;
                break;
            default:
                break;
        }
    }
}
