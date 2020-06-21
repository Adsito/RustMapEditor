using UnityEngine;
using XNode;
using RustMapEditor.Variables;

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
                MapManager.RotateLayer(LandLayers.Ground, CW);
                break;
            case NodeVariables.Texture.LandLayerEnum.Biome:
                MapManager.RotateLayer(LandLayers.Biome, CW);
                break;
            case NodeVariables.Texture.LandLayerEnum.Alpha:
                MapManager.RotateLayer(LandLayers.Alpha, CW);
                break;
            case NodeVariables.Texture.LandLayerEnum.Topology:
                MapManager.RotateTopologyLayers(topologies, CW);
                break;
        }
    }
}