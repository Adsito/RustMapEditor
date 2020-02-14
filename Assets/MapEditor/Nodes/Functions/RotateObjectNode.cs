using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Rotate/Rotate Objects")]
public class RotateObjectNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [NodeEnum] public NodeVariables.Misc.RotateDirection direction = NodeVariables.Misc.RotateDirection.ClockWise;
    [HideInInspector] public bool paths = true, prefabs = true;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        bool CW = (direction == NodeVariables.Misc.RotateDirection.ClockWise) ? true : false;
        if (paths)
        {
            MapManager.RotatePaths(CW);
        }
        if (prefabs)
        {
            MapManager.RotatePrefabs(CW);
        }
    }
}