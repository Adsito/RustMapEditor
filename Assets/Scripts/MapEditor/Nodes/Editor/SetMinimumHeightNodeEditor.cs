using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(SetMinimumHeightNode))]
public class SetMinimumHeightEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Set Minimum Height", "Sets any part of the heightmap below to the minimum height."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        SetMinimumHeightNode node = target as SetMinimumHeightNode;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Height", "The minimum height the heightmap should be. Will raise any part of the heightmap below this " +
            "height to the minimum height set."), GUILayout.MaxWidth(50));
        node.minimumHeight = EditorGUILayout.Slider(node.minimumHeight, 0f, 1000f);
        EditorGUILayout.EndHorizontal();
    }
}