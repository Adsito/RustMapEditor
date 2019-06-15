using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(PaintSlopeNode))]
public class PaintSlopeNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.magenta;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Paint Slope", "Paints the texture between the slope values. Can also blend the textures out on the Ground and Biome layers."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PaintSlopeNode node = target as PaintSlopeNode;
        AutoGenerationGraph graph = node.graph as AutoGenerationGraph;
        NodeVariables.Texture texture = (NodeVariables.Texture)node.GetValue();
        #region UpdateValues
        node.slopeLow = Mathf.Clamp(node.slopeLow, 0f, 89.99f);
        node.slopeMinBlendLow = Mathf.Clamp(node.slopeMinBlendLow, 0f, node.slopeLow);
        node.slopeMinBlendHigh = Mathf.Clamp(node.slopeMinBlendHigh, node.slopeMinBlendLow, node.slopeLow);
        node.slopeHigh = Mathf.Clamp(node.slopeHigh, 0.01f, 90f);
        node.slopeMaxBlendHigh = Mathf.Clamp(node.slopeMaxBlendHigh, node.slopeHigh, 90f);
        if (node.slopeLow > node.slopeHigh)
        {
            node.slopeLow = node.slopeHigh - 0.01f;
        }
        node.slopeMaxBlendLow = node.slopeLow;
        node.slopeMinBlendHigh = node.slopeHigh;
        if (node.blendSlopes == false)
        {
            node.slopeMaxBlendHigh = node.slopeHigh;
            node.slopeMinBlendLow = node.slopeLow;
        }
        #endregion
        GUILayout.Label("Slope Tools (Degrees)", EditorStyles.boldLabel); // From 0 - 90
        EditorGUILayout.BeginHorizontal();
        node.blendSlopes = EditorGUILayout.ToggleLeft("Blend Slopes", node.blendSlopes);
        // Todo: Toggle for check between heightrange.
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("From: ", EditorStyles.boldLabel, GUILayout.MaxWidth(41f));
        node.slopeLow = EditorGUILayout.FloatField(node.slopeLow, GUILayout.MaxWidth(50f));
        GUILayout.Label("To: ", EditorStyles.boldLabel, GUILayout.MaxWidth(23f));
        node.slopeHigh = EditorGUILayout.FloatField(node.slopeHigh, GUILayout.MaxWidth(50f));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref node.slopeLow, ref node.slopeHigh, 0f, 90f);
        if (node.blendSlopes == true)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Blend Low: ");
            node.slopeMinBlendLow = EditorGUILayout.FloatField(node.slopeMinBlendLow, GUILayout.MaxWidth(50f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref node.slopeMinBlendLow, ref node.slopeMaxBlendLow, 0f, 90f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Blend High: ");
            node.slopeMaxBlendHigh = EditorGUILayout.FloatField(node.slopeMaxBlendHigh, GUILayout.MaxWidth(50f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref node.slopeMinBlendHigh, ref node.slopeMaxBlendHigh, 0f, 90f);
        }
    }
}
