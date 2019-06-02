using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

[CreateNodeMenu("Paint/Paint Layer")]
public class PaintLayerNode : Node
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)] public NodeVariables.Texture In;
    [Output] public int nextTask;
    public override object GetValue(NodePort port)
    {
        NodeVariables.Texture In = GetInputValue("In", this.In);
        return In;
    }
    public object GetValue()
    {
        return GetInputValue<object>("In");
    }
}
