using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Rotate/Rotate Layer")]
public class RotateLayerNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public TerrainTopology.Enum topologies = TerrainTopology.NOTHING;
    [NodeEnum] public NodeVariables.Misc.RotateDirection direction = NodeVariables.Misc.RotateDirection.ClockWise;
    [NodeEnum] public NodeVariables.Texture.LandLayerEnum landLayer = NodeVariables.Texture.LandLayerEnum.Ground;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        bool CW = (direction == NodeVariables.Misc.RotateDirection.ClockWise) ? true : false;
        switch (landLayer)
        {
            case NodeVariables.Texture.LandLayerEnum.Ground:
                MapIO.RotateLayer("ground", CW);
                break;
            case NodeVariables.Texture.LandLayerEnum.Biome:
                MapIO.RotateLayer("biome", CW);
                break;
            case NodeVariables.Texture.LandLayerEnum.Alpha:
                MapIO.RotateLayer("alpha", CW);
                break;
            case NodeVariables.Texture.LandLayerEnum.Topology:
                MapIO.RotateTopologyLayers(topologies, CW);
                break;
        }
    }
}