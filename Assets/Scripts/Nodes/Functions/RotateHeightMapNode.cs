using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Rotate/Rotate HeightMap")]
public class RotateHeightMapNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    public NodeVariables.Misc.RotateDirection direction = NodeVariables.Misc.RotateDirection.ClockWise;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        bool CW = (direction == NodeVariables.Misc.RotateDirection.ClockWise) ? true : false;
        MapIO.RotateHeightmap(CW);
    }
}