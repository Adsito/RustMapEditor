using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/HeightMap/Set Maximum Height")]
public class SetMaximumHeightNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public float maximumHeight = 450f;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapIO.ClampHeightmap(0f, maximumHeight, RustMapEditor.Variables.Selections.Terrains.Land);
    }
}
