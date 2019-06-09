using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Set Minimum Height")]
public class SetMinimumHeight : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output] public NodeVariables.NextTask NextTask;
    [NonSerialized()] public float minimumHeight = 450f;
    public override object GetValue(NodePort port)
    {
        return null;
    }
    public void RunNode()
    {
        MapIO mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
        mapIO.setMinimumHeight(minimumHeight);
    }
}
