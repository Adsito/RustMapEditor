using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(PaintLayerNode))]
public class PaintLayerNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PaintLayerNode node = target as PaintLayerNode;
        AutoGenerationGraph graph = node.graph as AutoGenerationGraph;
        NodeVariables.Texture texture = node.GetValue();
        if (texture != null)
        {
            EditorGUILayout.IntField(texture.LandLayer);
        }
    }
}
