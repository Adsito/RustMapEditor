using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Invert/Invert HeightMap")]
public class InvertHeightMapNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [NodeEnum] public EditorVars.Selections.Terrains terrains = EditorVars.Selections.Terrains.Land;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapIO.InvertHeightmap(terrains);
    }
}