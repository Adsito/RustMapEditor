using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Rotate/Rotate HeightMap")]
public class RotateHeightMapNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public bool CW = true;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapIO.RotateHeightmap(CW);
    }
}