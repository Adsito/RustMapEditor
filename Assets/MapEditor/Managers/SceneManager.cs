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
            EditorApplication.update -= OnProjectLoad;
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