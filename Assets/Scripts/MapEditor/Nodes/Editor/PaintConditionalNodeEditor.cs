using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(PaintConditionalNode))]
public class PaintConditionalNodeEditor : NodeEditor
{
    int conditionalPaintOptions = 0;
    int[] values = { 0, 1 };
    string[] activeTextureAlpha = { "Visible", "Invisible" };
    string[] activeTextureTopo = { "Active", "Inactive" };
    string[] landLayers = { "Ground", "Biome", "Alpha", "Topology" };
    public override Color GetTint()
    {
        return Color.magenta;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Paint Conditional", "Paints the texture inputted into the node if it meets the conditions set below."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PaintConditionalNode node = target as PaintConditionalNode;
        NodeVariables.Texture texture = (NodeVariables.Texture)node.GetValue();
        GUILayout.Label("Conditional Paint", EditorStyles.boldLabel);

        GUIContent[] conditionalPaintMenu = new GUIContent[5];
        conditionalPaintMenu[0] = new GUIContent("Ground");
        conditionalPaintMenu[1] = new GUIContent("Biome");
        conditionalPaintMenu[2] = new GUIContent("Alpha");
        conditionalPaintMenu[3] = new GUIContent("Topology");
        conditionalPaintMenu[4] = new GUIContent("Terrain");
        conditionalPaintOptions = GUILayout.Toolbar(conditionalPaintOptions, conditionalPaintMenu);
        
        switch (conditionalPaintOptions)
        {
            case 0: // Ground
                GUILayout.Label("Ground Texture", EditorStyles.boldLabel);
                node.groundLayerConditions = (TerrainSplat.Enum)EditorGUILayout.EnumFlagsField(node.groundLayerConditions);
                break;
            case 1: // Biome
                GUILayout.Label("Biome Texture", EditorStyles.boldLabel);
                node.biomeLayerConditions = (TerrainBiome.Enum)EditorGUILayout.EnumFlagsField(node.biomeLayerConditions);
                break;
            case 2: // Alpha
                node.checkAlpha = EditorGUILayout.ToggleLeft("Check Alpha:", node.checkAlpha);
                if (node.checkAlpha)
                {
                    node.alphaTexture = EditorGUILayout.IntPopup("Texture:", node.alphaTexture, activeTextureAlpha, values);
                }
                break;
            case 3: // Topology
                GUILayout.Label("Topology Layer", EditorStyles.boldLabel);
                node.topologyLayerConditions = (TerrainTopology.Enum)EditorGUILayout.EnumFlagsField(node.topologyLayerConditions);
                EditorGUILayout.Space();
                GUILayout.Label("Topology Texture", EditorStyles.boldLabel);
                node.topologyTexture = EditorGUILayout.IntPopup("Texture:", node.topologyTexture, activeTextureTopo, values);
                break;
            case 4: // Terrain
                GUILayout.Label("Slope Range", EditorStyles.boldLabel);
                node.checkSlope = EditorGUILayout.Toggle("Check Slopes:", node.checkSlope);
                if (node.checkSlope == true)
                {
                    if (node.slopeLowCndtl > node.slopeHighCndtl)
                    {
                        node.slopeLowCndtl = node.slopeHighCndtl - 0.05f;
                    }
                    if (node.slopeLowCndtl < 0)
                    {
                        node.slopeLowCndtl = 0f;
                    }
                    if (node.slopeHighCndtl > 90f)
                    {
                        node.slopeHighCndtl = 90f;
                    }
                    EditorGUILayout.BeginHorizontal();
                    node.slopeLowCndtl = EditorGUILayout.FloatField(node.slopeLowCndtl);
                    node.slopeHighCndtl = EditorGUILayout.FloatField(node.slopeHighCndtl);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.MinMaxSlider(ref node.slopeLowCndtl, ref node.slopeHighCndtl, 0f, 90f);
                }
                GUILayout.Label("Height Range", EditorStyles.boldLabel);
                node.checkHeight = EditorGUILayout.Toggle("Check Heights:", node.checkHeight);
                if (node.checkHeight == true)
                {
                    if (node.heightLowCndtl > node.heightHighCndtl)
                    {
                        node.heightLowCndtl = node.heightHighCndtl - 0.05f;
                    }
                    if (node.heightLowCndtl < 0)
                    {
                        node.heightLowCndtl = 0f;
                    }
                    if (node.heightHighCndtl > 1000f)
                    {
                        node.heightHighCndtl = 1000f;
                    }
                    EditorGUILayout.BeginHorizontal();
                    node.heightLowCndtl = EditorGUILayout.FloatField(node.heightLowCndtl);
                    node.heightHighCndtl = EditorGUILayout.FloatField(node.heightHighCndtl);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.MinMaxSlider(ref node.heightLowCndtl, ref node.heightHighCndtl, 0f, 1000f);
                }
                break;
        }
    }
}