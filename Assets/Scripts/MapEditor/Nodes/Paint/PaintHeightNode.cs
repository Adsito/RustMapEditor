using UnityEngine;
using XNode;
using EditorVariables;

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
            case 0: 
                MapIO.PaintHeightBlend(LandLayers.Ground, heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: 
                MapIO.PaintHeightBlend(LandLayers.Biome, heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: 
                MapIO.PaintHeight(LandLayers.Alpha, heightLow, heightHigh, layer.AlphaTexture);
                break;
            case 3: 
                MapIO.PaintHeight(LandLayers.Topology, heightLow, heightHigh, layer.TopologyTexture, layer.TopologyLayer);
                break;
        }
    }
}