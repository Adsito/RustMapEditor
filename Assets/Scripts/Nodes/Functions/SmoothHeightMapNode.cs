using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/HeightMap/Smooth HeightMap")]
public class SmoothHeightMapNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public float filterStrength = 1f, blurDirection = 0f;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapIO mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
        mapIO.SmoothHeightmap(filterStrength, blurDirection);
    }
}