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
        }
    }

    private static void OnMouseDown()
    {
        if (Event.current.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
            {
                PrefabManager.Spawn(hit.point);
            }
        }
    }
}