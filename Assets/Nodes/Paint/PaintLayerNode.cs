using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

[CreateNodeMenu("Paint/Paint Layer")]
public class PaintLayerNode : Node
{
    [Input] public InputAllTypes textureInput;
    [Output] public NextTask nextTask;
    public override object GetValue(NodePort port)
    {
        return nextTask;
    }
}