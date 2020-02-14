using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class BundleManager
{
    private static bool loaded = false;

    private const string manifestPath = "assets/manifest.asset";

    public static AssetBundleBackend Backend { get; private set; }
    public static GameManifest Manifest { get; private set; }

    public static bool IsLoaded()
    {
        return loaded;
    }
    /// <summary>Loads the prefabs from the Rust prefab bundle.</summary>
    /// <param name="bundlename">The file path to the bundle.</param>
    public static void Load(string bundlename)
    {
        if (!loaded)
        {
            Backend = new AssetBundleBackend(bundlename);
            Manifest = Backend.Load<GameManifest>(manifestPath);
            if (Manifest == null)
            {
                Debug.LogError("Manifest is null");
                Dispose();
                return;
            }
            AssetDump();
            loaded = true;
        }
        else
        {
            Debug.Log("Prefabs already loaded!");
        }
    }
    /// <summary>Disposes the loaded bundle and prefabs.</summary>
    public static void Dispose()
    {
        if (loaded)
        {
            loaded = false;
            Backend.cache.Clear();
            Backend.Dispose();
        }
    }
    public static List<string> GetManifestStrings()
    {
        if (!loaded)
        {
            return null;
        }
        List<string> manifestStrings = new List<string>();
        foreach (var item in Manifest.pooledStrings)
        {
            manifestStrings.Add(item.str);
        }
        return manifestStrings;
    }
    /// <summary>Dumps every asset found in the Rust content bundle to a text file.</summary>
    static void AssetDump()
    {
        using (StreamWriter streamWriter = new StreamWriter("AssetDump.txt", false))
        {
            foreach (var item in Backend.FindAll(""))
            {
                streamWriter.WriteLine(item + " : " + StringPool.Get(item));
            }
        }
    }
}