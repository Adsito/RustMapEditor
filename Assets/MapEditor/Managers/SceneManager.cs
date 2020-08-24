using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManager
{
    public static Scene EditorScene { get; private set; }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.update += OnProjectLoad;
        EditorApplication.hierarchyChanged += OnSceneChanged;
    }

    private static void OnProjectLoad()
    {
        EditorScene = EditorSceneManager.GetActiveScene();
        if (EditorScene.IsValid())
        {
            CentreSceneView(SceneView.lastActiveSceneView);
            SetCullingDistances(SceneView.GetAllSceneCameras(), SettingsManager.PrefabRenderDistance, SettingsManager.PathRenderDistance);
            SetClippingDistances(SceneView.sceneViews);
            EditorApplication.update -= OnProjectLoad;
        }
    }

    public static void SetCullingDistances(Camera[] camera, float prefabDist, float pathDist)
    {
        float[] distances = new float[32];
        distances[8] = prefabDist;
        distances[9] = pathDist;
        foreach (var item in camera)
            item.layerCullDistances = distances;

        SceneView.RepaintAll();
    }

    public static void CentreSceneView(SceneView sceneView)
    {
        if (sceneView != null)
        {
            sceneView.orthographic = false;
            sceneView.pivot = new Vector3(500f, 600f, 500f);
            sceneView.rotation = Quaternion.Euler(25f, 0f, 0f);
        }
    }

    public static void SetClippingDistances(ArrayList sceneViews)
    {
        foreach (SceneView item in sceneViews)
            if (item.cameraSettings.nearClip < 0.5f)
                item.cameraSettings.nearClip = 0.5f;
    }

    private static void OnSceneChanged()
    {
        if (EditorScene.rootCount != 4)
        {
            foreach (var item in EditorScene.GetRootGameObjects())
            {
                if (item.TryGetComponent(out PrefabDataHolder prefab))
                {
                    prefab.gameObject.transform.SetParent(PrefabManager.PrefabParent);
                    Selection.activeObject = prefab;
                    continue;
                }
                if (item.TryGetComponent(out PathDataHolder path))
                    path.gameObject.transform.SetParent(PathManager.PathParent);
            }
        }
    }

    public static void ToggleHideFlags(bool enabled)
    {
        foreach (var item in GameObject.FindObjectsOfType<SceneObjectHideFlags>())
            item.ToggleHideFlags(enabled);

        SceneHierarchyHooks.ReloadAllSceneHierarchies();
    }
}