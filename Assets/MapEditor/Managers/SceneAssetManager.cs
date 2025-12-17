using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static WorldSerialization;

public static class SceneAssetManager
{
    public static AssetSceneManifest Manifest { get; private set; }

    // Maps prefab path (lowercase) -> scene name
    public static Dictionary<string, string> PrefabToSceneLookup { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    // Maps scene name -> SceneEntry (for CanUnload check)
    public static Dictionary<string, SceneEntry> SceneEntryLookup { get; private set; } = new Dictionary<string, SceneEntry>(StringComparer.OrdinalIgnoreCase);

    // Currently loaded scenes (scene name -> Scene handle)
    public static Dictionary<string, Scene> LoadedScenes { get; private set; } = new Dictionary<string, Scene>(StringComparer.OrdinalIgnoreCase);

    // Cached GameObjects from scenes (prefab path -> GameObject)
    public static Dictionary<string, GameObject> ScenePrefabCache { get; private set; } = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

    // All prefab paths from manifest (for UI listing)
    public static List<string> ManifestPrefabPaths { get; private set; } = new List<string>();

    public static bool IsManifestLoaded { get; private set; }
    public static bool IsLoadingScene { get; private set; }

    /// <summary>Parses the JSON manifest and builds lookup tables.</summary>
    public static void ParseManifest(string json)
    {
        try
        {
            Manifest = JsonUtility.FromJson<AssetSceneManifest>(json);
            if (Manifest == null || Manifest.Scenes == null)
            {
                Debug.LogError("Failed to parse AssetSceneManifest: Invalid JSON structure");
                return;
            }
            BuildLookupTables();
            IsManifestLoaded = true;
            Debug.Log($"SceneAssetManager: Loaded manifest with {Manifest.Scenes.Count} scenes, {ManifestPrefabPaths.Count} prefabs");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse AssetSceneManifest: {e.Message}");
        }
    }

    private static void BuildLookupTables()
    {
        PrefabToSceneLookup.Clear();
        SceneEntryLookup.Clear();
        ManifestPrefabPaths.Clear();
        LoadedScenes.Clear();
        ScenePrefabCache.Clear();

        foreach (var scene in Manifest.Scenes)
        {
            SceneEntryLookup[scene.Name] = scene;

            if (scene.IncludedAssets != null)
            {
                foreach (var prefabPath in scene.IncludedAssets)
                {
                    if (!string.IsNullOrEmpty(prefabPath))
                    {
                        PrefabToSceneLookup[prefabPath] = scene.Name;
                        ManifestPrefabPaths.Add(prefabPath);
                    }
                }
            }
        }

        ManifestPrefabPaths.Sort();
    }

    /// <summary>Ensures the scene containing the prefab is loaded and the prefab is cached.</summary>
    public static IEnumerator EnsurePrefabAvailable(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
            yield break;

        // Check if prefab already cached
        if (ScenePrefabCache.ContainsKey(prefabPath))
            yield break;

        if (!PrefabToSceneLookup.TryGetValue(prefabPath, out string sceneName))
        {
            Debug.LogWarning($"SceneAssetManager: Prefab not found in manifest: {prefabPath}");
            yield break;
        }

        // Check if scene already loaded
        if (!LoadedScenes.ContainsKey(sceneName))
        {
            yield return LoadSceneAdditive(sceneName);
        }

        // Cache the prefab from the loaded scene
        CachePrefabFromScene(sceneName, prefabPath);
    }

    private static IEnumerator LoadSceneAdditive(string sceneName)
    {
        // Wait if another scene is loading (enforce sequential)
        while (IsLoadingScene)
            yield return null;

        IsLoadingScene = true;

        string scenePath = GetScenePathFromBundle(sceneName);
        if (string.IsNullOrEmpty(scenePath))
        {
            Debug.LogError($"SceneAssetManager: Scene not found in bundles: {sceneName}");
            IsLoadingScene = false;
            yield break;
        }

        Debug.Log($"SceneAssetManager: Loading scene: {sceneName}");

        AsyncOperation asyncOp = null;
        try
        {
            asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
        }
        catch (Exception e)
        {
            Debug.LogError($"SceneAssetManager: Failed to load scene {sceneName}: {e.Message}");
            IsLoadingScene = false;
            yield break;
        }

        if (asyncOp == null)
        {
            Debug.LogError($"SceneAssetManager: LoadSceneAsync returned null for: {scenePath}");
            IsLoadingScene = false;
            yield break;
        }

        while (!asyncOp.isDone)
            yield return null;

        var loadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(scenePath);
        if (loadedScene.IsValid())
        {
            LoadedScenes[sceneName] = loadedScene;
            Debug.Log($"SceneAssetManager: Successfully loaded scene: {sceneName}");
        }
        else
        {
            Debug.LogError($"SceneAssetManager: Scene loaded but invalid: {sceneName}");
        }

        IsLoadingScene = false;
    }

    private static void CachePrefabFromScene(string sceneName, string prefabPath)
    {
        if (!LoadedScenes.TryGetValue(sceneName, out Scene scene))
        {
            Debug.LogWarning($"SceneAssetManager: Scene not loaded when caching prefab: {sceneName}");
            return;
        }

        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogWarning($"SceneAssetManager: Scene invalid or not loaded: {sceneName}");
            return;
        }

        // Search for GameObject with matching name (full path)
        foreach (var rootObj in scene.GetRootGameObjects())
        {
            GameObject found = FindGameObjectByName(rootObj, prefabPath);
            if (found != null)
            {
                ScenePrefabCache[prefabPath] = found;
                return;
            }
        }

        Debug.LogWarning($"SceneAssetManager: Prefab '{prefabPath}' not found in scene '{sceneName}'");
    }

    private static GameObject FindGameObjectByName(GameObject root, string name)
    {
        if (root.name.Equals(name, StringComparison.OrdinalIgnoreCase))
            return root;

        foreach (Transform child in root.transform)
        {
            var found = FindGameObjectByName(child.gameObject, name);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>Gets a cached prefab from a loaded scene.</summary>
    public static GameObject GetPrefabFromScene(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
            return null;

        if (ScenePrefabCache.TryGetValue(prefabPath, out GameObject prefab))
            return prefab;

        return null;
    }

    /// <summary>Tries to unload scenes that are no longer needed.</summary>
    public static IEnumerator TryUnloadUnusedScenes(PrefabData[] allPrefabs, int currentIndex)
    {
        if (allPrefabs == null || currentIndex < 0)
            yield break;

        // Build set of scenes still needed by remaining prefabs
        HashSet<string> scenesStillNeeded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = currentIndex + 1; i < allPrefabs.Length; i++)
        {
            string path = AssetManager.ToPath(allPrefabs[i].id);
            if (!string.IsNullOrEmpty(path) && PrefabToSceneLookup.TryGetValue(path, out string sceneName))
            {
                scenesStillNeeded.Add(sceneName);
            }
        }

        // Find scenes that can be unloaded
        List<string> scenesToUnload = new List<string>();
        foreach (var kvp in LoadedScenes)
        {
            string sceneName = kvp.Key;
            if (!scenesStillNeeded.Contains(sceneName) &&
                SceneEntryLookup.TryGetValue(sceneName, out SceneEntry entry) &&
                entry.CanUnload)
            {
                scenesToUnload.Add(sceneName);
            }
        }

        // Unload the scenes
        foreach (string sceneName in scenesToUnload)
        {
            yield return UnloadScene(sceneName);
        }
    }

    private static IEnumerator UnloadScene(string sceneName)
    {
        if (!LoadedScenes.TryGetValue(sceneName, out Scene scene))
            yield break;

        // Remove cached prefabs from this scene
        List<string> keysToRemove = new List<string>();
        foreach (var kvp in PrefabToSceneLookup)
        {
            if (kvp.Value.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            ScenePrefabCache.Remove(key);
        }

        Debug.Log($"SceneAssetManager: Unloading scene: {sceneName}");

        if (scene.IsValid() && scene.isLoaded)
        {
            var asyncOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
            if (asyncOp != null)
            {
                while (!asyncOp.isDone)
                    yield return null;
            }
        }

        LoadedScenes.Remove(sceneName);
    }

    /// <summary>Gets the scene path in the bundle from the scene name.</summary>
    private static string GetScenePathFromBundle(string sceneName)
    {
        // Scene name format: "AssetScene-bootstrap" -> "AssetScene-bootstrap.unity"
        string searchName = sceneName + ".unity";

        foreach (var kvp in AssetManager.BundleLookup)
        {
            // Check if the path ends with the scene file name
            if (kvp.Key.EndsWith(searchName, StringComparison.OrdinalIgnoreCase))
                return kvp.Key;
        }

        // Also try searching for just the scene name in the path
        foreach (var kvp in AssetManager.BundleLookup)
        {
            if (kvp.Key.Contains(sceneName) && kvp.Key.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                return kvp.Key;
        }

        return null;
    }

    /// <summary>Disposes all loaded scenes and clears caches.</summary>
    public static void Dispose()
    {
        // Unload all loaded scenes
        foreach (var scene in LoadedScenes.Values)
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                try
                {
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"SceneAssetManager: Failed to unload scene: {e.Message}");
                }
            }
        }

        Manifest = null;
        PrefabToSceneLookup.Clear();
        SceneEntryLookup.Clear();
        LoadedScenes.Clear();
        ScenePrefabCache.Clear();
        ManifestPrefabPaths.Clear();
        IsManifestLoaded = false;
        IsLoadingScene = false;

        Debug.Log("SceneAssetManager: Disposed");
    }
}
