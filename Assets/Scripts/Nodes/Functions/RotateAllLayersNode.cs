using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Rotate/Rotate All Layers")]
public class RotateAllLayersNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output] public NodeVariables.NextTask NextTask;
    [NonSerialized()] public bool CW = true;
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
                mapIO.RotateGroundmap(CW);
                break;
            case 1: // Biome
                mapIO.RotateBiomemap(CW);
                break;
            case 2: // Alpha
                mapIO.RotateAlphamap(CW);
                break;
            case 3: // Topology
                mapIO.RotateAllTopologymap(CW);
                break;
        }
    }
}
