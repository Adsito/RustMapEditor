using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(SetMaximumHeight))]
public class SetMaximumHeightEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Set Maximum Height", "Sets any part of the heightmap above to the maximum height."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        SetMaximumHeight node = target as SetMaximumHeight;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Height", "The maximum height the heightmap should be. Will lower any part of the heightmap above this " +
            "height to the maximum height set."), GUILayout.MaxWidth(50));
        node.maximumHeight = EditorGUILayout.Slider(node.maximumHeight, 0f, 1000f);
        EditorGUILayout.EndHorizontal();
    }
}