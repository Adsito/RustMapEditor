using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Paint Layer")]
public class PaintLayerNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
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
                MapIO.PaintLayer("Ground", TerrainSplatSDK.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                MapIO.PaintLayer("Biome", TerrainBiomeSDK.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                MapIO.PaintLayer("Alpha", layer.AlphaTexture);
                break;
            case 3: // Topology
                MapIO.PaintLayer("Topology", layer.TopologyTexture, layer.TopologyLayer);
                break;
            default:
                break;
        }
    }
}
