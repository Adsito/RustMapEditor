using UnityEngine;
using XNodeEditor;
using RustMapEditor.UI;
using RustMapEditor.Variables;

[CustomNodeEditor(typeof(PaintConditionalNode))]
public class PaintConditionalNodeEditor : NodeEditor
{
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
        if (node.conditions.GroundConditions.Layer == TerrainSplat.NOTHING)
        {
            node.conditions = new Conditions()
            {
                GroundConditions = new GroundConditions(TerrainSplat.Enum.Grass),
                BiomeConditions = new BiomeConditions(TerrainBiome.Enum.Temperate),
                TopologyConditions = new TopologyConditions(TerrainTopology.Enum.Beach)
            };
        }
        if (node.layers == null)
        {
            node.layers = new Layers() { Ground = TerrainSplat.Enum.Grass, Biome = TerrainBiome.Enum.Temperate, Topologies = TerrainTopology.Enum.Field };
        }
        Functions.ConditionalPaintConditions(ref node.conditions, ref node.cndOptions);
    }
}