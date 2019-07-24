using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Paint Rivers")]
public class PaintRiversNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public bool aboveTerrain = false;
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
                mapIO.PaintRiver("Ground", aboveTerrain, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                mapIO.PaintRiver("Biome", aboveTerrain, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                mapIO.PaintRiver("Alpha", aboveTerrain, layer.AlphaTexture);
                break;
            case 3: // Topology
                mapIO.PaintRiver("Topology", aboveTerrain, layer.TopologyTexture, layer.TopologyLayer);
                break;
        }
    }
}