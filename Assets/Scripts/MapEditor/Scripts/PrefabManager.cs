using UnityEngine;
using System.Collections.Generic;
using System.IO;
using RustMapEditor.Data;
using static WorldSerialization;

public static class PrefabManager
{
	private static AssetBundleBackend backend;
    private static GameManifest manifest;

    static List<string> assetsList = new List<string>();

    private const string manifestPath = "assets/manifest.asset";
    public static bool prefabsLoaded = false;

    /// <summary>Loads the prefabs from the Rust prefab bundle.</summary>
    /// <param name="bundlename">The file path to the bundle.</param>
    public static void LoadBundle(string bundlename)
    {
        if (!prefabsLoaded)
        {
            backend = new AssetBundleBackend(bundlename);
            manifest = backend.Load<GameManifest>(manifestPath);
            if (manifest == null)
            {
                Debug.LogError("Manifest is null");
                backend.Dispose();
                prefabsLoaded = false;
                return;
            }
            AssetDump();
            prefabsLoaded = true;
        }
        else
        {
            Debug.Log("Prefabs already loaded!");
        }
    }
    /// <summary>Disposes the loaded bundle and prefabs.</summary>
    public static void DisposeBundle()
    {
        if (prefabsLoaded)
        {
            prefabsLoaded = false;
            backend.Dispose();
        }
    }
    public static GameManifest GetManifest()
    {
        return manifest;
    }
    public static List<string> GetManifestStrings()
    {
        if (!prefabsLoaded)
        {
            return null;
        }
        List<string> manifestStrings = new List<string>();
        foreach (var item in manifest.pooledStrings)
        {
            manifestStrings.Add(item.str);
        }
        return manifestStrings;
    }
    /// <summary>Loads, sets up and returns the prefab at the asset path.</summary>
    /// <param name="path">The Prefab path in the bundle file.</param>
    public static GameObject LoadPrefab(string path)
    {
        return backend.LoadPrefab(path);
    }
    /// <summary>Loads, sets up and returns the prefab at the prefab id.</summary>
    /// <param name="id">The prefab manifest id.</param>
    public static GameObject LoadPrefab(uint id)
    {
        return LoadPrefab(StringPool.Get(id));
    }
    public static void SpawnPrefab(GameObject go, PrefabData prefabData, Transform parent = null)
    {
        GameObject newObj = GameObject.Instantiate(go, parent);
        newObj.transform.position = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z) + LandData.GetMapOffset();
        newObj.transform.rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        newObj.transform.localScale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        newObj.name = go.name;
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
    }
    /// <summary>Gathers a list of strings for every asset found in the content bundle file.</summary>
    static void AssetBundleLookup() 
    {
        var assets = backend.FindAll("");
        assetsList.Clear();
        foreach (var asset in assets)
        {
            assetsList.Add(asset);
        }
    }
    /// <summary>Dumps every asset found in the Rust content bundle to a text file.</summary>
    static void AssetDump() 
    {
        AssetBundleLookup();
        using (StreamWriter streamWriter = new StreamWriter("AssetDump.txt", false))
        {
            foreach (var item in assetsList)
            {
                streamWriter.WriteLine(item +  " : " + StringPool.Get(item));
            }
        }
    }
    /// <summary>Prepare the loaded prefab for use with the map editor.</summary>
    /// <param name="go">The prefab GameObject.</param>
    /// <param name="path">The prefab path.</param>
    public static GameObject PreparePrefab(GameObject go)
    {
        go.SetLayerRecursively(8);
        go.AddComponent<PrefabDataHolder>();
        return go;
    }
}