using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(PaintHeightNode))]
public class PaintHeightNodeEditor : NodeEditor
{
    bool blendHeights = false;
    public override Color GetTint()
    {
        return Color.magenta;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Paint Height", "Paints the texture between the height values. Can also blend the textures out on the Ground and Biome layers."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PaintHeightNode node = target as PaintHeightNode;
        NodeVariables.Texture texture = (NodeVariables.Texture)node.GetValue();
        #region UpdateValues
        node.heightLow = Mathf.Clamp(node.heightLow, 0f, 999.99f);
        node.heightMinBlendLow = Mathf.Clamp(node.heightMinBlendLow, 0f, node.heightLow);
        node.heightMinBlendHigh = Mathf.Clamp(node.heightMinBlendHigh, node.heightMinBlendLow, node.heightLow);
        node.heightHigh = Mathf.Clamp(node.heightHigh, 0.01f, 1000f);
        node.heightMaxBlendHigh = Mathf.Clamp(node.heightMaxBlendHigh, node.heightHigh, 1000f);
        if (node.heightLow > node.heightHigh)
        {
            node.heightLow = node.heightHigh - 0.01f;
        }
        node.heightMaxBlendLow = node.heightLow;
        node.heightMinBlendHigh = node.heightHigh;
        if (blendHeights == false)
        {
            node.heightMaxBlendHigh = node.heightHigh;
            node.heightMinBlendLow = node.heightLow;
        }
        #endregion
        GUILayout.Label("Height Tools (Metres)", EditorStyles.boldLabel); // From 0 - 90
        blendHeights = EditorGUILayout.ToggleLeft("Blend Heights", blendHeights);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("From: ", EditorStyles.boldLabel, GUILayout.MaxWidth(41f));
        node.heightLow = EditorGUILayout.FloatField(node.heightLow, GUILayout.MaxWidth(50f));
        GUILayout.Label("To: ", EditorStyles.boldLabel, GUILayout.MaxWidth(23f));
        node.heightHigh = EditorGUILayout.FloatField(node.heightHigh, GUILayout.MaxWidth(50f));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref node.heightLow, ref node.heightHigh, 0f, 1000f);
        if (blendHeights == true)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Blend Low: ");
            node.heightMinBlendLow = EditorGUILayout.FloatField(node.heightMinBlendLow, GUILayout.MaxWidth(50f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref node.heightMinBlendLow, ref node.heightMaxBlendLow, 0f, 1000f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Blend High: ");
            node.heightMaxBlendHigh = EditorGUILayout.FloatField(node.heightMaxBlendHigh, GUILayout.MaxWidth(50f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref node.heightMinBlendHigh, ref node.heightMaxBlendHigh, 0f, 1000f);
        }
    }
}
