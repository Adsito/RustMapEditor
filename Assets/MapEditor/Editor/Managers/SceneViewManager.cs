using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SceneViewManager : Editor
{
    static SceneViewManager()
    {
        SceneView.beforeSceneGui += OnUpdate;
    }

    private static void OnUpdate(SceneView sceneView)
    {
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                OnMouseDown();
                break;
            case EventType.DragExited:
                OnDragExited();
                break;
        }
    }

    public static bool GetMouseScenePos(out RaycastHit hit)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        bool castHit = Physics.Raycast(ray, out RaycastHit hitInfo, 10000f);
        hit = hitInfo;
        return castHit;
    }

    private static void SpawnPrefab()
    {
        if (GetMouseScenePos(out RaycastHit hit))
            PrefabManager.Spawn(hit.point);
    }

    private static void OnMouseDown()
    {
        if (Event.current.button == 0)
            SpawnPrefab();
    }

    private static void OnDragExited()
    {
        SpawnPrefab();
    }
}