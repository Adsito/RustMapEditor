using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class BundleManager
{
    private const string manifestPath = "assets/manifest.asset";

    public static AssetBundleBackend Backend { get; private set; }
    public static GameManifest Manifest { get; private set; }

    public static bool IsLoaded { get; private set; }

    /// <summary>Loads the prefabs from the Rust prefab bundle.</summary>
    /// <param name="bundlename">The file path to the bundle.</param>
    public static void Load(string bundlename)
    {
        if (!IsLoaded)
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
            IsLoaded = true;
        }
        else
        {
            Debug.Log("Prefabs already loaded!");
        }
    }
    /// <summary>Disposes the loaded bundle and prefabs.</summary>
    public static void Dispose()
    {
        if (IsLoaded)
        {
            IsLoaded = false;
            Backend.cache.Clear();
            Backend.Dispose();
        }
    }
    public static List<string> GetManifestStrings()
    {
        if (!IsLoaded)
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