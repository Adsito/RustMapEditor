using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(RotateLayerNode))]
public class RotateLayerNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Rotate Layer", "Rotates the layer of the inputted texture."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        RotateLayerNode node = target as RotateLayerNode;
        node.CW = EditorGUILayout.ToggleLeft(new GUIContent(node.CW ? "Rotate Direction: 90°" : "Rotate Direction: 270°", "The direction which the layer will rotate, either Clockwise or CounterClockwise"), node.CW);
    }
}