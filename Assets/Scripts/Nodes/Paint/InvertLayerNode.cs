using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Paint/Invert Layer")]
public class InvertLayerNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture Texture;
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output] public NodeVariables.NextTask NextTask;
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
            case 2: // Alpha
                mapIO.changeLayer("Alpha");
                mapIO.InvertLayer("Alpha");
                break;
            case 3: // Topology. Going to overhaul the topology layers soon to avoid all the changing of layer values.
                mapIO.changeLayer("Topology");
                mapIO.oldTopologyLayer2 = mapIO.topologyLayer;

                mapIO.topologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(layer.TopologyLayer);
                mapIO.changeLandLayer();
                mapIO.oldTopologyLayer = (TerrainTopology.Enum)TerrainTopology.IndexToType(layer.TopologyLayer);
                mapIO.InvertLayer("Topology");

                mapIO.topologyLayer = mapIO.oldTopologyLayer2;
                mapIO.changeLandLayer();
                mapIO.oldTopologyLayer = mapIO.oldTopologyLayer2;
                break;
        }
    }
}
