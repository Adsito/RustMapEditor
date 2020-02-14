using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/HeightMap/Set Minimum Height")]
public class SetMinimumHeightNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public float minimumHeight = 450f;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapManager.ClampHeightmap(minimumHeight, 1000f, RustMapEditor.Variables.Selections.Terrains.Land);
    }
}