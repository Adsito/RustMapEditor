using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Paint Slope")]
public class PaintSlopeNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public float slopeLow = 40f, slopeHigh = 60f, slopeMinBlendLow = 25f, slopeMaxBlendLow = 40f, slopeMinBlendHigh = 60f, slopeMaxBlendHigh = 75f;
    [HideInInspector] public bool blendSlopes = false;
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
                MapIO.PaintSlope("Ground", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainSplatSDK.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                MapIO.PaintSlope("Biome", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainBiomeSDK.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                MapIO.PaintSlope("Alpha", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, layer.AlphaTexture);
                break;
            case 3: // Topology
                MapIO.PaintSlope("Topology", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, layer.TopologyTexture, layer.TopologyLayer);
                break;
        }
    }
}