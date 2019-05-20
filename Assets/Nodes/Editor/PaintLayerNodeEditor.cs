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
        NodeVariables.Texture texture = (NodeVariables.Texture)node.GetValue();
        if (texture != null)
        {
            EditorGUILayout.LabelField(texture.LandLayer.ToString());
            EditorGUILayout.LabelField(texture.TopologyLayer.ToString());
            EditorGUILayout.LabelField(texture.TopologyTexture.ToString());
        }
    }
}
