using UnityEngine;
using XNode;
using RustMapEditor.Variables;

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
            case 0:
                MapIO.PaintSlopeBlend(LandLayers.Ground, slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: 
                MapIO.PaintSlopeBlend(LandLayers.Biome, slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: 
                MapIO.PaintSlope(LandLayers.Alpha, slopeLow, slopeHigh, layer.AlphaTexture);
                break;
            case 3: 
                MapIO.PaintSlope(LandLayers.Topology, slopeLow, slopeHigh, layer.TopologyTexture, layer.TopologyLayer);
                break;
        }
    }
}