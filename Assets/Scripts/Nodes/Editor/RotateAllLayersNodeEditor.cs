using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(RotateAllLayersNode))]
public class RotateAllLayersNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Rotate All Layers", "Rotates all the layers of the selected landlayer."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        RotateAllLayersNode node = target as RotateAllLayersNode;
        node.CW = EditorGUILayout.ToggleLeft(new GUIContent(node.CW ? "Rotate Direction: 90°" : "Rotate Direction: 270°", "The direction which the layer will rotate, either 90° or 270°"), node.CW);
    }
}