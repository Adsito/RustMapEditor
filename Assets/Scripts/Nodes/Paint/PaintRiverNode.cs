using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Paint River")]
public class PaintRiverNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output] public NodeVariables.NextTask NextTask;
    [NonSerialized()] public bool aboveTerrain = false;
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
        switch (layer.LandLayer)
        {
            case 0: // Ground
                mapIO.changeLayer("Ground");
                mapIO.PaintRiver("Ground", aboveTerrain, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                mapIO.changeLayer("Biome");
                mapIO.PaintRiver("Biome", aboveTerrain, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                mapIO.changeLayer("Alpha");
                mapIO.PaintRiver("Alpha", aboveTerrain, layer.AlphaTexture);
                break;
            case 3: // Topology. Going to overhaul the topology layers soon to avoid all the changing of layer values.
                mapIO.changeLayer("Topology");
                mapIO.oldTopologyLayer2 = mapIO.topologyLayer;

                mapIO.topologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(layer.TopologyLayer);
                mapIO.changeLandLayer();
                mapIO.oldTopologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(layer.TopologyLayer);
                mapIO.PaintRiver("Topology", aboveTerrain, layer.TopologyTexture);

                mapIO.topologyLayer = mapIO.oldTopologyLayer2;
                mapIO.changeLandLayer();
                mapIO.oldTopologyLayer = mapIO.oldTopologyLayer2;
                break;
        }
    }
}