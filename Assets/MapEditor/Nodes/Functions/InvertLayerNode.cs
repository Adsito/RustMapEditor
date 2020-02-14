using UnityEngine;
using XNode;
using RustMapEditor.Variables;

[CreateNodeMenu("Functions/Invert/Invert Layer")]
public class InvertLayerNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public TerrainTopology.Enum topologies = TerrainTopology.NOTHING;
    [NodeEnum] public NodeVariables.Misc.DualLayerEnum layer = NodeVariables.Misc.DualLayerEnum.Topology;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        switch (layer)
        {
            case NodeVariables.Misc.DualLayerEnum.Alpha:
                MapManager.InvertLayer(LandLayers.Alpha);
                break;
            case NodeVariables.Misc.DualLayerEnum.Topology:
                MapManager.InvertTopologyLayers(topologies);
                break;
        }
    }
}