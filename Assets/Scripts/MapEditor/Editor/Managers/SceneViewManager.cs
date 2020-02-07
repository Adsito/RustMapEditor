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
                OnMouseDown(sceneView);
                break;
        }
    }

    private static void OnMouseDown(SceneView sceneView)
    {
        if (Event.current.button == 0)
        {
            Vector2 screenPixelPos = HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition);
            Ray ray = sceneView.camera.ScreenPointToRay(screenPixelPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 10000f, LayerMask.GetMask("Water", "UI", "Paths")))
            {
                PrefabManager.Spawn(hit.point);
            }
        }
    }
}