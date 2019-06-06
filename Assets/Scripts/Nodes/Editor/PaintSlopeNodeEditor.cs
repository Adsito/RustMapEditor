using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(PaintSlopeNode))]
public class PaintSlopeNodeEditor : NodeEditor
{
    bool blendSlopes = false;
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PaintSlopeNode node = target as PaintSlopeNode;
        AutoGenerationGraph graph = node.graph as AutoGenerationGraph;
        NodeVariables.Texture texture = (NodeVariables.Texture)node.GetValue();
        #region UpdateValues
        if (node.slopeMinBlendHigh > node.slopeMaxBlendHigh)
        {
            node.slopeMaxBlendHigh = node.slopeMinBlendHigh + 0.25f;
            if (node.slopeMaxBlendHigh > 90f)
            {
                node.slopeMaxBlendHigh = 90f;
            }
        }
        if (node.slopeMinBlendLow > node.slopeMaxBlendLow)
        {
            node.slopeMinBlendLow = node.slopeMaxBlendLow - 0.25f;
            if (node.slopeMinBlendLow < 0f)
            {
                node.slopeMinBlendLow = 0f;
            }
        }
        node.slopeMaxBlendLow = node.slopeLow;
        node.slopeMinBlendHigh = node.slopeHigh;
        if (blendSlopes == false)
        {
            node.slopeMinBlendLow = node.slopeMaxBlendLow;
            node.slopeMaxBlendHigh = node.slopeMinBlendHigh;
        }
        #endregion
        if (texture != null && Event.current.type != EventType.DragExited) // Check for mouse control event error.
        {
            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
            EditorGUILayout.BeginHorizontal();
            blendSlopes = EditorGUILayout.ToggleLeft("Toggle Blend Slopes", blendSlopes);
            // Todo: Toggle for check between heightrange.
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("From: " + node.slopeLow.ToString() + "°", EditorStyles.boldLabel, GUILayout.MaxWidth(90f));
            GUILayout.Label("To: " + node.slopeHigh.ToString() + "°", EditorStyles.boldLabel, GUILayout.MaxWidth(90f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref node.slopeLow, ref node.slopeHigh, 0f, 90f);
            if (blendSlopes == true)
            {
                GUILayout.Label("Blend Low: " + node.slopeMinBlendLow + "°");
                EditorGUILayout.MinMaxSlider(ref node.slopeMinBlendLow, ref node.slopeMaxBlendLow, 0f, 90f);
                GUILayout.Label("Blend High: " + node.slopeMaxBlendHigh + "°");
                EditorGUILayout.MinMaxSlider(ref node.slopeMinBlendHigh, ref node.slopeMaxBlendHigh, 0f, 90f);
            }
        }
    }
}
