using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PrefabAttributes : List<PrefabAttributes>
{
    public GameObject Prefab
    {
        get; set;
    }
    public string Path
    {
        get; set;
    }
    public uint RustID
    {
        get; set;
    }
}
public static class PrefabManager
{
	private static AssetBundleBackend backend;
	private static HashLookup lookup;
    public static List<Mesh> meshes = new List<Mesh>();
    public static List<string> assetsList = new List<string>();
    public static List<PrefabAttributes> prefabsList = new List<PrefabAttributes>();
    public static Dictionary<uint, GameObject> prefab = new Dictionary<uint, GameObject>();

    private static string manifestPath = "assets/manifest.asset";
    private static string assetsToLoadPath = "AssetsList.txt";
    public static bool prefabsLoaded = false;

    public static void PrefabLookup(string bundlename)
    {
        backend = new AssetBundleBackend(bundlename);
        AssetBundleLookup();
        AssetDump();
        var lookupString = "";
        var manifest = backend.Load<GameManifest>(manifestPath);
        if (manifest == null)
        {
            Debug.LogError("Manifest is null");
            backend = null;
            return;
        }
        else
        {
            for (uint index = 0; (long)index < (long)manifest.pooledStrings.Length; ++index)
            {
                lookupString += "0," + manifest.pooledStrings[index].hash + "," + manifest.pooledStrings[index].str + "\n";
            }
        }
        lookup = new HashLookup(lookupString);
        var lines = File.ReadAllLines(assetsToLoadPath);
        float progressInterval = 1f / lines.Length;
        float progress = 0f;
        foreach (var line in lines)
        {
            MapIO.ProgressBar("Prefab Warmup", "Loading Directory: " + line, progress);
            progress += progressInterval;
            if (line.EndsWith("/") || line.EndsWith("\\"))
            {
                LoadPrefabs(line);
            }
        }
        PrefabsLoadedDump();
        MapIO.ClearProgressBar();
        prefabsLoaded = true;
    }
    private static void LoadPrefabs(string path)
    {
        var subpaths = backend.FindAll(path);
        for (int i = 0; i < subpaths.Length; i++)
        {
            if (subpaths[i].Contains(".prefab") && subpaths[i].Contains(".item") == false)
            {
                PreparePrefab(backend.Load<GameObject>(subpaths[i]), subpaths[i], lookup[subpaths[i]]);
            }
        }
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
        using (StreamWriter streamWriter = new StreamWriter("AssetDump.txt", false))
        {
            foreach (var item in assetsList)
            {
                streamWriter.WriteLine(item);
            }
        }
    }
    /// <summary>
    /// Dumps a list of all the prefabs loaded to a file.
    /// </summary>
    static void PrefabsLoadedDump()
    {
        using(StreamWriter streamWriter = new StreamWriter("PrefabsLoaded.txt", false))
        {
            foreach (var item in prefabsList)
            {
                streamWriter.WriteLine(item.Prefab.name + ":" + item.Path + ":" + item.RustID);
            }
        }
    }
    private static void PreparePrefab(GameObject go, string path, uint rustid) // Seperates the prefab components and adds them to list.
    {
        var prefabPath = path.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        go.SetActive(true);
        go.tag = "LoadedPrefab";
        go.name = prefabName;
        PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new WorldSerialization.PrefabData
        {
            id = rustid,
        };
        prefab.Add(rustid, go);
        prefabsList.Add(new PrefabAttributes()
        {
            Prefab = go,
            Path = path,
            RustID = rustid
        });
    }
}