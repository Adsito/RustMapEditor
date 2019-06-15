using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Paint Slope")]
public class PaintSlopeNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output] public NodeVariables.NextTask NextTask;
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
        MapIO mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
        if (layer == null) // Check for if the textures node is not connected.
        {
            return;
        }
        switch (layer.LandLayer)
        {
            case 0: // Ground
                mapIO.PaintSlope("Ground", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: // Biome
                mapIO.PaintSlope("Biome", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: // Alpha
                mapIO.PaintSlope("Alpha", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, layer.AlphaTexture);
                break;
            case 3: // Topology
                mapIO.PaintSlope("Topology", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, layer.TopologyTexture, layer.TopologyLayer);
                break;
        }
    }
}