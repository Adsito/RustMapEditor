using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.InputSystem;

[EditorTool("Rotate Tool Centred")]
class RotateToolCentred : EditorTool
{
    Quaternion LastRotation { get; set; } = Quaternion.identity;

    GUIContent m_IconContent;

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = EditorGUIUtility.IconContent("d_RotateTool").image,
            text = "Rotate Tool Centred",
            tooltip = "Rotate Tool Centred"
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

        Quaternion Rotation = Quaternion.identity;
        if (CentredToolManager.SelectionLength > 1 || Tools.pivotRotation == PivotRotation.Global)
        {
            EditorGUI.BeginChangeCheck();
            Rotation = Handles.RotationHandle(Mouse.current.leftButton.isPressed ? LastRotation : Quaternion.identity, CentredToolManager.HandlePos);
            if (EditorGUI.EndChangeCheck())
            {
                Quaternion Delta = Rotation * Quaternion.Inverse(LastRotation);
                Undo.RecordObjects(Selection.transforms, "Rotate Tool Centred");
                foreach (var transform in Selection.transforms)
                    transform.Rotate(Quaternion.RotateTowards(transform.rotation, Delta, 180f).eulerAngles, Space.World);
            }
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            Rotation = Handles.RotationHandle(CentredToolManager.TransformRotation, CentredToolManager.HandlePos);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Selection.activeTransform, "Rotate Tool Centred");
                Selection.activeTransform.rotation = Quaternion.RotateTowards(Selection.activeTransform.rotation, Rotation, 180f);
            }
        }
        LastRotation = Rotation;
        if (!Mouse.current.leftButton.isPressed)
            LastRotation = Rotation;
    }
}