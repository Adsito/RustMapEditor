using UnityEditor;
using UnityEngine;

public static class CentredToolManager
{
    public static Quaternion TransformRotation { get => Selection.transforms.Length == 1 ? Selection.activeTransform.rotation : Quaternion.identity; }

    public static Vector3 HandlePos 
    { 
        get => HandleUtility.GUIPointToWorldRay(new Vector2(SceneView.lastActiveSceneView.camera.pixelWidth, SceneView.lastActiveSceneView.camera.pixelHeight) / 2).GetPoint(10f);
    }

    public static bool ObjectsSelected { get => Selection.transforms.Length > 0 ? true : false; }

    public static int SelectionLength { get => Selection.transforms.Length; }
}