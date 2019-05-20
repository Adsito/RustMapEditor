using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

[CreateNodeMenu("Paint/Paint Layer")]
public class PaintLayerNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture In;
    [Output] public NextTask nextTask;
    public NodeVariables.Texture GetValue()
    {
        return In;
    }
}
