using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(PaintRiversNode))]
public class PaintRiverNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.magenta;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Paint River", "Paints the texture wherever the water is above 500."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PaintRiversNode node = target as PaintRiversNode;
        node.aboveTerrain = EditorGUILayout.ToggleLeft(new GUIContent("Above Terrain", "When ticked, only paints when the water is above 500 AND is above the height of" +
            "the terrain."), node.aboveTerrain);
    }
}