using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class BundleManager
{
    private static bool loaded = false;

    private const string manifestPath = "assets/manifest.asset";

    public static AssetBundleBackend Backend;
    private static GameManifest manifest;

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
            manifest = Backend.Load<GameManifest>(manifestPath);
            if (manifest == null)
            {
                Debug.LogError("Manifest is null");
                Backend.Dispose();
                loaded = false;
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
            Backend.Dispose();
        }
    }
    public static GameManifest GetManifest()
    {
        return manifest;
    }
    public static List<string> GetManifestStrings()
    {
        if (!loaded)
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