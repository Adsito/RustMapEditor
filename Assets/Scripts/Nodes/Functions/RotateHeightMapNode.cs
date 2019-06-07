using System;
using UnityEngine;
using XNode;

[CreateNodeMenu("Functions/Rotate HeightMap")]
public class RotateHeightMapNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.NextTask PreviousTask;
    [Output] public NodeVariables.NextTask NextTask;
    [NonSerialized()] public bool CW = true;
    public void RunNode()
    {
        MapIO mapIO = GameObject.FindGameObjectWithTag("MapIO").GetComponent<MapIO>();
        mapIO.rotateHeightmap(CW);
    }
}
