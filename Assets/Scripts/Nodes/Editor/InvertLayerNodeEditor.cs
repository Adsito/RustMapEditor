using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(InvertLayerNode))]
public class InvertLayerNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.magenta;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Invert Layer", "Inverts the Alpha and Topology layer textures."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
}