using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.InputSystem;

[EditorTool("Scale Tool Centred")]
class ScaleToolCentred : EditorTool
{
    Vector3 LastScale { get; set; }

    GUIContent m_IconContent;

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = EditorGUIUtility.IconContent("d_ScaleTool").image,
            text = "Scale Tool Centred",
            tooltip = "Scale Tool Centred"
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
        Vector3 Scale = Vector3.one;
        Scale = Handles.ScaleHandle(Scale, CentredToolManager.HandlePos, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : CentredToolManager.TransformRotation, 1.5f);
        if (EditorGUI.EndChangeCheck())
        {
            Vector3 Delta = (Scale - LastScale) * 2f;
            Undo.RecordObjects(Selection.transforms, "Scale Tool Centred");
            foreach (var transform in Selection.transforms)
                transform.localScale += Delta;
            LastScale = Scale;
        }
        if (!Mouse.current.leftButton.isPressed)
            LastScale = Vector3.one;
    }
}