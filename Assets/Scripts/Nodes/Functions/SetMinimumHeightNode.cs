using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/HeightMap/Set Minimum Height")]
public class SetMinimumHeightNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask NextTask;
    [HideInInspector] public float minimumHeight = 450f;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapIO mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
        mapIO.SetMinimumHeight(minimumHeight);
    }
}