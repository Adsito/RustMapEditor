using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public static class PrefabManager
{
	private static AssetBundleBackend backend;
    private static GameManifest manifest;

    static List<string> assetsList = new List<string>();
    static List<string> prefabsPrepared = new List<string>();
    static List<string> LODs = new List<string>();

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
            prefabsPrepared.Clear();
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
    /// <returns>The prefab loaded, or the default prefab if unable to load.</returns>
    public static GameObject LoadPrefab(string path)
    {
        if (prefabsPrepared.Contains(path))
        {
            return backend.LoadPrefab(path); 
        }
        return PreparePrefab(backend.Load<GameObject>(path), path, StringPool.Get(path));
    }
    /// <summary>Loads, sets up and returns the prefab at the prefab id.</summary>
    /// <param name="id">The prefab manifest id.</param>
    public static GameObject LoadPrefab(uint id)
    {
        return LoadPrefab(StringPool.Get(id));
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
    public static void AssetLODDump()
    {
        using (StreamWriter streamWriter = new StreamWriter("RendererLODs.txt", false))
        {
            try
            {
                var manifestStrings = GetManifestStrings();
                Debug.Log(manifestStrings.Count);
                for (int i = 0; i < manifestStrings.Count; i++)
                {
                    GameObject tempObject = backend.Load<GameObject>(manifestStrings[i]);
                    if (tempObject == null)
                    {
                        continue;
                    }
                    tempObject.SetActive(true);
                    foreach (var item in tempObject.GetComponentsInChildren<RendererLOD>())
                    {
                        if (LODs.Contains(item.States[0].renderer.name))
                        {
                            continue;
                        }
                        LODs.Add(item.States[0].renderer.name);
                        for (int j = 0; j < item.States.Length; j++)
                        {
                            if (item.States[j].renderer != null)
                            {
                                streamWriter.WriteLine(item.States[j].renderer.name + " : " + item.States[j].distance);
                            }
                            else
                            {
                                streamWriter.WriteLine("Culling Distance : " + item.States[j].distance);
                            }
                        }
                    }
                    foreach (var item in tempObject.GetComponentsInChildren<MeshLOD>())
                    {
                        if (LODs.Contains(item.States[0].mesh.name))
                        {
                            continue;
                        }
                        LODs.Add(item.States[0].mesh.name);
                        for (int j = 0; j < item.States.Length; j++)
                        {
                            if (item.States[j].mesh != null)
                            {
                                streamWriter.WriteLine(item.States[j].mesh.name + " : " + item.States[j].distance);
                            }
                            else
                            {
                                streamWriter.WriteLine("Culling Distance : " + item.States[j].distance);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Log(ex.StackTrace);
            }
        }
    }
    /// <summary>Prepare the loaded prefab for use with the map editor.</summary>
    /// <param name="go">The prefab GameObject.</param>
    /// <param name="path">The prefab path.</param>
    private static GameObject PreparePrefab(GameObject go, string path, uint rustid)
    {
        var prefabPath = path.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        foreach (var renderer in go.GetComponentsInChildren<RendererLOD>())
        {
            renderer.SetLODs();
        }
        foreach (var transform in go.GetComponentsInChildren<Transform>())
        {
            transform.gameObject.layer = 8;
        }
        go.SetActive(true);
        go.tag = "LoadedPrefab";
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