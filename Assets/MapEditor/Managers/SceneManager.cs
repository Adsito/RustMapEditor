using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManager
{
    #region Init
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
            CentreSceneView();
            SetCullingDistances(SettingsManager.PrefabRenderDistance, SettingsManager.PathRenderDistance);
            SetClippingDistances();
            EditorApplication.update -= OnProjectLoad;
        }
    }
    #endregion

    public static Scene EditorScene { get; private set; }

    #region Scene Camera
    /// <summary>Sets/Updates the all SceneViews with inputted culling distances.</summary>
    /// <param name="prefabDist">Distance to cull prefabs, in meters.</param>
    /// <param name="pathDist">Distance to cull path nodes, in meters.</param>
    public static void SetCullingDistances(float prefabDist, float pathDist) => SetCullingDistances(SceneView.GetAllSceneCameras(), prefabDist, pathDist);

    /// <summary>Sets/Updates the selected SceneViews with inputted culling distances.</summary>
    /// <param name="camera">The Cameras which will have culling distances updated.</param>
    /// <param name="prefabDist">Distance to cull prefabs, in meters.</param>
    /// <param name="pathDist">Distance to cull path nodes, in meters.</param>
    public static void SetCullingDistances(Camera[] camera, float prefabDist, float pathDist)
    {
        float[] distances = new float[32];
        distances[8] = prefabDist;
        distances[9] = pathDist;
        foreach (var item in camera)
        {
            item.layerCullDistances = distances;
            item.layerCullSpherical = true;
        }

        SceneView.RepaintAll();
    }

    /// <summary>Centres the last active SceneView on the terrain object.</summary>
    public static void CentreSceneView() => CentreSceneView(SceneView.lastActiveSceneView);

    /// <summary>Centres the selected SceneView on the terrain object.</summary>
    /// <param name="sceneView">SceneView to centre.</param>
    public static void CentreSceneView(SceneView sceneView)
    {
        if (sceneView != null)
        {
            sceneView.orthographic = false;
            sceneView.pivot = new Vector3(500f, 600f, 500f);
            sceneView.rotation = Quaternion.Euler(25f, 0f, 0f);
        }
    }

    /// <summary>Centres all open SceneViews on the terrain object.</summary>
    public static void CentreSceneViews()
    {
        foreach (var view in SceneView.sceneViews)
            CentreSceneView(view as SceneView);
    }

    /// <summary>Sets/Updates the selected SceneViews with inputted clipping distances.</summary>
    public static void SetClippingDistances() => SetClippingDistances(SceneView.sceneViews);

    /// <summary>Sets/Updates the selected SceneViews with inputted clipping distances.</summary>
    /// <param name="sceneViews">The SceneViews which will have culling distances updated.</param>
    public static void SetClippingDistances(ArrayList sceneViews)
    {
        foreach (SceneView item in sceneViews)
            if (item.cameraSettings.nearClip < 0.5f)
                item.cameraSettings.nearClip = 0.5f;
    }
    #endregion

    #region Other
    /// <summary>Gets the currently selected root map prefabs.</summary>
    /// <returns>Array of PrefabDataHolders attached to currently selected prefabs.</returns>
    public static PrefabDataHolder[] GetSelectedPrefabs()
    {
        var prefabs = new List<PrefabDataHolder>();
        foreach (var item in Selection.gameObjects)
        {
            if (item.TryGetComponent(out PrefabDataHolder holder))
                prefabs.Add(holder);
        }
        return prefabs.ToArray();
    }

    /// <summary>Toggles the HideFlags on Scene objects.</summary>
    /// <param name="enabled">True = Hidden / False = Visible</param>
    public static void ToggleHideFlags(bool enabled)
    {
        foreach (var item in GameObject.FindObjectsOfType<SceneObjectHideFlags>())
            item.ToggleHideFlags(enabled);

        SceneHierarchyHooks.ReloadAllSceneHierarchies();
    }

    /// <summary>Called when active scene hierarchy is modified.</summary>
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
                if (item.TryGetComponent(out PrefabCategoryParent categoryParent))
                    categoryParent.gameObject.transform.SetParent(PrefabManager.PrefabParent);
            }
        }
    }
    #endregion
}