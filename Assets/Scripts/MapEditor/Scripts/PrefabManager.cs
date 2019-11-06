using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class PrefabManager
{
	private static AssetBundleBackend backend;
    public static GameManifest manifest;

    public static List<string> assetsList = new List<string>();
    public static List<string> prefabsPrepared = new List<string>();

    private const string manifestPath = "assets/manifest.asset";
    public static bool prefabsLoaded = false;

    /// <summary>
    /// Loads the prefabs from the Rust prefab bundle.
    /// </summary>
    /// <param name="bundlename">The file path to the bundle.</param>
    public static void LoadBundle(string bundlename)
    {
        if (!prefabsLoaded)
        {
            backend = new AssetBundleBackend(bundlename);
            AssetDump();
            manifest = backend.Load<GameManifest>(manifestPath);
            if (manifest == null)
            {
                Debug.LogError("Manifest is null");
                backend.Dispose();
                prefabsLoaded = false;
                return;
            }
            prefabsLoaded = true;
        }
        else
        {
            Debug.Log("Prefabs already loaded!");
        }
    }
    /// <summary>
    /// Disposes the loaded bundle and prefabs.
    /// </summary>
    public static void DisposeBundle()
    {
        if (prefabsLoaded)
        {
            prefabsPrepared.Clear();
            prefabsLoaded = false;
            backend.Dispose();
        }
    }
    /// <summary>
    /// Loads, sets up and returns the prefab at the asset path.
    /// </summary>
    /// <param name="path">The Prefab path in the bundle file.</param>
    /// <returns>The prefab loaded, or the default prefab if unable to load.</returns>
    public static GameObject LoadPrefab(string path)
    {
        if (prefabsPrepared.Contains(path))
        {
            return backend.LoadPrefab(path); 
        }
        return PreparePrefab(backend.Load<GameObject>(path), path, StringPool.Get(path));
    }
    /// <summary>
    /// Loads, sets up and returns the prefab at the prefab id.
    /// </summary>
    /// <param name="id">The prefab manifest id.</param>
    /// <returns></returns>
    public static GameObject LoadPrefab(uint id)
    {
        return LoadPrefab(StringPool.Get(id));
    }
    /// <summary>
    /// Gathers a list of strings for every asset found in the content bundle file.
    /// </summary>
    static void AssetBundleLookup() 
    {
        var assets = backend.FindAll("");
        assetsList.Clear();
        foreach (var asset in assets)
        {
            assetsList.Add(asset);
        }
    }
    /// <summary>
    /// Dumps every asset found in the Rust content bundle to a text file.
    /// </summary>
    static void AssetDump() 
    {
        AssetBundleLookup();
        using (StreamWriter streamWriter = new StreamWriter("AssetDump.txt", false))
        {
            foreach (var item in assetsList)
            {
                streamWriter.WriteLine(item);
            }
        }
    }
    /// <summary>
    /// Prepare the loaded prefab for use with the map editor.
    /// </summary>
    /// <param name="go">The prefab GameObject.</param>
    /// <param name="path">The prefab path.</param>
    /// <returns></returns>
    private static GameObject PreparePrefab(GameObject go, string path, uint rustid)
    {
        var prefabPath = path.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        foreach (var renderer in go.GetComponentsInChildren<RendererLOD>())
        {
            renderer.SetLODS();
        }
        go.SetActive(true);
        go.tag = "LoadedPrefab";
        go.layer = 8;
        go.name = prefabName;
        PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new WorldSerialization.PrefabData
        {
            id = rustid,
        };
        prefabsPrepared.Add(path);
        return go;
    }
}