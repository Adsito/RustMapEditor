using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
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
        AutoGenerationGraph graph = node.graph as AutoGenerationGraph;
        NodeVariables.Texture texture = (NodeVariables.Texture)node.GetValue();
        #region UpdateValues
        if (node.heightMinBlendLow > node.heightMaxBlendLow)
        {
            node.heightMinBlendLow = node.heightMaxBlendLow - 0.25f;
            if (node.heightMinBlendLow < 0f)
            {
                node.heightMinBlendLow = 0f;
            }
        }
        if (node.heightMinBlendHigh > node.heightMaxBlendHigh)
        {
            node.heightMaxBlendHigh = node.heightMinBlendHigh + 0.25f;
            if (node.heightMaxBlendHigh > 1000f)
            {
                node.heightMaxBlendHigh = 1000f;
            }
        }
        node.heightMaxBlendLow = node.heightLow;
        node.heightMinBlendHigh = node.heightHigh;
        if (blendHeights == false)
        {
            node.heightMinBlendLow = node.heightLow;
            node.heightMaxBlendHigh = node.heightHigh;
        }
        #endregion
        GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 90
        blendHeights = EditorGUILayout.ToggleLeft("Toggle Blend Heights", blendHeights);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("From: " + node.heightLow.ToString() + "m", EditorStyles.boldLabel);
        GUILayout.Label("To: " + node.heightHigh.ToString() + "m", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref node.heightLow, ref node.heightHigh, 0f, 1000f);
        if (blendHeights == true)
        {
            GUILayout.Label("Blend Low: " + node.heightMinBlendLow + "m");
            EditorGUILayout.MinMaxSlider(ref node.heightMinBlendLow, ref node.heightMaxBlendLow, 0f, 1000f);
            GUILayout.Label("Blend High: " + node.heightMaxBlendHigh + "m");
            EditorGUILayout.MinMaxSlider(ref node.heightMinBlendHigh, ref node.heightMaxBlendHigh, 0f, 1000f);
        }
    }
}
