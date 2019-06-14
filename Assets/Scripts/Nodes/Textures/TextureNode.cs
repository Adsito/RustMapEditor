using XNode;

[CreateNodeMenu("Texture")]
public class TextureNode : Node
{
    [Output] public NodeVariables.Texture Texture;
    public NodeVariables.Texture.LandLayerEnum landLayer;
    public override object GetValue(NodePort port)
    {
        return Texture;
    }
}