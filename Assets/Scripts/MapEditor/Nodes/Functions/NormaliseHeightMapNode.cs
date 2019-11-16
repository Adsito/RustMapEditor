using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/HeightMap/Normalise HeightMap")]
public class NormaliseHeightMapNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public float normaliseLow = 450f, normaliseHigh = 1000f;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapIO.NormaliseHeightmap(normaliseLow, normaliseHigh, RustMapEditor.Variables.Selections.Terrains.Land);
    }
}
