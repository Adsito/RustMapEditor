using UnityEngine;
using XNode;
using RustMapEditor.Variables;

[CreateNodeMenu("Functions/Clear/Clear Layer")]
public class ClearLayerNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector, NodeEnum] public TerrainTopology.Enum topologies = TerrainTopology.NOTHING;
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
                MapManager.ClearLayer(LandLayers.Alpha);
                break;
            case NodeVariables.Misc.DualLayerEnum.Topology:
                MapManager.ClearTopologyLayers(topologies);
                break;
        }
    }
}