using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Rotate/Rotate HeightMap")]
public class RotateHeightMapNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [NodeEnum] public NodeVariables.Misc.RotateDirection direction = NodeVariables.Misc.RotateDirection.ClockWise;
    [NodeEnum] public EditorVars.Selections.Terrains terrains = EditorVars.Selections.Terrains.Land;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        bool CW = (direction == NodeVariables.Misc.RotateDirection.ClockWise) ? true : false;
        MapIO.RotateHeightMap(CW, terrains);
    }
}