using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(TextureNode))]
public class TextureNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.green;   
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Texture", "Select the texture from this node, and input it into any accepting nodes to choose the texture to paint."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        TextureNode node = target as TextureNode;
        AutoGenerationGraph graph = node.graph as AutoGenerationGraph;
        switch (node.landLayer)
        {
            case NodeVariables.Texture.LandLayerEnum.Ground:
                node.groundEnum = (TerrainSplat.Enum)EditorGUILayout.EnumPopup("Texture", node.groundEnum);
                break;
            case NodeVariables.Texture.LandLayerEnum.Biome:
                node.biomeEnum = (TerrainBiome.Enum)EditorGUILayout.EnumPopup("Texture", node.biomeEnum);
                break;
            case NodeVariables.Texture.LandLayerEnum.Alpha:
                node.alphaEnum = (NodeVariables.Texture.AlphaEnum)EditorGUILayout.EnumPopup("Texture", node.alphaEnum);
                break;
            case NodeVariables.Texture.LandLayerEnum.Topology:
                node.topologyLayer = (TerrainTopology.Enum)EditorGUILayout.EnumPopup("Topology", node.topologyLayer);
                node.topologyEnum = (NodeVariables.Texture.TopologyEnum)EditorGUILayout.EnumPopup("Texture", node.topologyEnum);
                break;
        }
        node.Texture.LandLayer = (int)node.landLayer;
        node.Texture.TopologyLayer = TerrainTopology.TypeToIndex((int)node.topologyLayer);
        node.Texture.GroundTexture = (int)node.groundEnum;
        node.Texture.BiomeTexture = (int)node.biomeEnum;
        node.Texture.AlphaTexture = (int)node.alphaEnum;
        node.Texture.TopologyTexture = (int)node.topologyEnum;
    }
}
