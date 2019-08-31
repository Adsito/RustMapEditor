using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Paint Height")]
public class PaintHeightNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public float heightLow = 500f, heightHigh = 750f, heightMinBlendLow = 250f, heightMaxBlendLow = 500f, heightMinBlendHigh = 750f, heightMaxBlendHigh = 1000f;
    [HideInInspector] public bool blendHeights = false;
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
                MapIO.PaintHeight("Ground", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainSplatSDK.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                MapIO.PaintHeight("Biome", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainBiomeSDK.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                MapIO.PaintHeight("Alpha", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, layer.AlphaTexture);
                break;
            case 3: // Topology. Going to overhaul the topology layers soon to avoid all the changing of layer values.
                MapIO.PaintHeight("Topology", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, layer.TopologyTexture, layer.TopologyLayer);
                break;
        }
    }
}
