using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

[CreateNodeMenu("Paint/Paint Layer")]
public class PaintLayerNode : Node
{
    [Input] public Anything textureInput;
    [Output] public Anything nextTask;
    public override object GetValue(NodePort port)
    {
        return nextTask;
    }
}