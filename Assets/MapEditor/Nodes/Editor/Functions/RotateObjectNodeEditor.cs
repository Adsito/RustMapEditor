using UnityEngine;
using UnityEditor;
using XNodeEditor;

[CustomNodeEditor(typeof(RotateObjectNode))]
public class RotateObjectNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        return Color.cyan;
    }
    public override void OnHeaderGUI()
    {
        GUILayout.Label(new GUIContent("Rotate Objects", "Rotates the selected map objects."), NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        RotateObjectNode node = target as RotateObjectNode;
        node.prefabs = EditorGUILayout.ToggleLeft(new GUIContent("Rotate Prefabs", "Rotates the maps prefabs by the direction selected."), node.prefabs);
        node.paths = EditorGUILayout.ToggleLeft(new GUIContent("Rotate Paths", "Rotates the maps paths by the direction selected."), node.paths);
    }
}