using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using NodeVariables;

[CreateNodeMenu("Textures/Biome Texture")]
public class BiomeTextureNode : Node
{
    [Output] public BiomeTexture Out;
    public BiomeTexture Texture;
    public override object GetValue(NodePort port)
    {
        return Out;
    }
}