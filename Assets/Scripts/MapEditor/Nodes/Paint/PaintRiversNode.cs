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
        switch (layer.LandLayer)
        {
            case 0: // Ground
                MapIO.PaintRiver("Ground", aboveTerrain, TerrainSplatSDK.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                MapIO.PaintRiver("Biome", aboveTerrain, TerrainBiomeSDK.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                MapIO.PaintRiver("Alpha", aboveTerrain, layer.AlphaTexture);
                break;
            case 3: // Topology
                MapIO.PaintRiver("Topology", aboveTerrain, layer.TopologyTexture, layer.TopologyLayer);
                break;
        }
    }
}