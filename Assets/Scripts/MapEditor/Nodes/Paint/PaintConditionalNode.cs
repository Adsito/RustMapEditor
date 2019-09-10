using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Paint Conditional")]
[NodeWidth(350)]
public class PaintConditionalNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    #region Fields
    [HideInInspector] public TerrainSplat.Enum groundLayerConditions = TerrainSplat.NOTHING, groundLayerToPaint = TerrainSplat.Enum.Grass;
    [HideInInspector] public TerrainBiome.Enum biomeLayerConditions = TerrainBiome.NOTHING, biomeLayerToPaint = TerrainBiome.Enum.Temperate;
    [HideInInspector] public TerrainTopology.Enum topologyLayerConditions = TerrainTopology.NOTHING, topologyLayerToPaint = TerrainTopology.Enum.Beach;
    [HideInInspector] public int topologyTexture = 0, alphaTexture = 0, layerConditionalInt = 0, texture = 0;
    [HideInInspector] public bool checkAlpha = false, checkHeight = false, checkSlope = false;
    [HideInInspector] public float slopeLowCndtl = 45f, slopeHighCndtl = 60f, heightLowCndtl = 500f, heightHighCndtl = 600f;
    #endregion
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
        Conditions conditions = new Conditions();
        conditions.GroundConditions = groundLayerConditions;
        conditions.BiomeConditions = biomeLayerConditions;
        conditions.CheckAlpha = checkAlpha;
        conditions.AlphaTexture = alphaTexture;
        conditions.TopologyLayers = topologyLayerConditions;
        conditions.TopologyTexture = topologyTexture;
        conditions.CheckSlope = checkSlope;
        conditions.SlopeLow = slopeLowCndtl;
        conditions.SlopeHigh = slopeHighCndtl;
        conditions.CheckHeight = checkHeight;
        conditions.HeightLow = heightLowCndtl;
        conditions.HeightHigh = heightHighCndtl;
        var layer = (NodeVariables.Texture)GetValue();
        if (layer == null) // Check for if the textures node is not connected.
        {
            return;
        }
        switch (layer.LandLayer)
        {
            case 0: // Ground
                MapIO.PaintConditional("Ground", TerrainSplat.TypeToIndex(layer.GroundTexture), conditions);
                break;
            case 1: // Biome
                MapIO.PaintConditional("Biome", TerrainBiome.TypeToIndex(layer.BiomeTexture), conditions);
                break;
            case 2: // Alpha
                MapIO.PaintConditional( "Alpha", layer.AlphaTexture, conditions);
                break;
            case 3: // Topology.
                MapIO.PaintConditional("Topology", layer.TopologyTexture, conditions, layer.TopologyLayer);
                break;
        }
    }
}