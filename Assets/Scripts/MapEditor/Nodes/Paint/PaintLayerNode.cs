using UnityEngine;
using XNode;
using EditorVariables;

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
            case 0: 
                MapIO.PaintLayer(LandLayers.Ground, TerrainSplat.TypeToIndex(layer.GroundTexture));
                break;
            case 1: 
                MapIO.PaintLayer(LandLayers.Biome, TerrainBiome.TypeToIndex(layer.BiomeTexture));
                break;
            case 2: 
                MapIO.PaintLayer(LandLayers.Alpha, layer.AlphaTexture);
                break;
            case 3: 
                MapIO.PaintLayer(LandLayers.Topology, layer.TopologyTexture, layer.TopologyLayer);
                break;
        }
    }
}
