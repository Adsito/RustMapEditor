using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.InputSystem;

[EditorTool("Move Tool Centred")]
class MoveToolCentred : EditorTool
{
    Vector3 LastPosition { get; set; }

    GUIContent m_IconContent;

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = EditorGUIUtility.IconContent("d_MoveTool").image,
            text = "Move Tool Centred",
            tooltip = "Move Tool Centred"
        };
    }

    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!CentredToolManager.ObjectsSelected)
            return;

        EditorGUI.BeginChangeCheck();
        Vector3 Position = Handles.PositionHandle(CentredToolManager.HandlePos, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : CentredToolManager.TransformRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Vector3 Delta = Position - LastPosition;
            Undo.RecordObjects(Selection.transforms, "Move Tool Centred");
            foreach (var transform in Selection.transforms)
                transform.position += Delta;
            LastPosition = Position;
        }
        if (!Mouse.current.leftButton.isPressed)
            LastPosition = CentredToolManager.HandlePos;
    }
}