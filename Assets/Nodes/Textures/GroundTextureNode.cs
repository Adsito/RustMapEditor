using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

[CreateNodeMenu("Textures/Ground Texture")]
public class GroundTextureNode : Node
{
    [Output] public GroundTexture Out;
    public GroundTexture Texture;
    public override object GetValue(NodePort port)
    {
        return Out = Texture;
    }
}