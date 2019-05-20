using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

[CreateNodeMenu("Texture")]
public class TextureNode : Node
{
    [Output] public NodeVariables.Texture Out;
    public NodeVariables.Texture.LandLayerEnum landLayer;
    public override object GetValue(NodePort port)
    {
        Out.LandLayer = (int)landLayer;
        return Out.LandLayer;
    }
}