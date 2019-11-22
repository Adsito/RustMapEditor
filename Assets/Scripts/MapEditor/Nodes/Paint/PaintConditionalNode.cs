using UnityEngine;
using XNode;
using RustMapEditor.Variables;

[CreateNodeMenu("Paint/Paint Conditional")]
[NodeWidth(350)]
public class PaintConditionalNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    #region Fields
    [HideInInspector] public int cndOptions, texture;
    [HideInInspector] public Conditions conditions;
    [HideInInspector] public Layers layers;
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
        var layer = (NodeVariables.Texture)GetValue();
        if (layer == null)
        {
            return;
        }
        switch (layer.LandLayer)
        {
            case 0: 
                MapIO.PaintConditional(LandLayers.Ground, TerrainSplat.TypeToIndex(layer.GroundTexture), conditions);
                break;
            case 1: 
                MapIO.PaintConditional(LandLayers.Biome, TerrainBiome.TypeToIndex(layer.BiomeTexture), conditions);
                break;
            case 2: 
                MapIO.PaintConditional(LandLayers.Alpha, layer.AlphaTexture, conditions);
                break;
            case 3: 
                MapIO.PaintConditional(LandLayers.Topology, layer.TopologyTexture, conditions, layer.TopologyLayer);
                break;
        }
    }
}