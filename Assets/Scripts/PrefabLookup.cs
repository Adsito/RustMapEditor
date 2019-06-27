using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
public class PrefabLookup : System.IDisposable
{
	private AssetBundleBackend backend;
	private HashLookup lookup;
    public List<Mesh> meshes = new List<Mesh>();
    public List<string> assetsList = new List<string>();
    public List<PrefabAttributes> prefabsList = new List<PrefabAttributes>();

    private static string manifestPath = "assets/manifest.asset";
    private static string assetsToLoadPath = "AssetsList.txt";
    public bool prefabsLoaded = false;

    public bool isLoaded
	{
		get { return prefabsLoaded; }
	}

    public PrefabLookup(string bundlename, MapIO mapIO)
    {
        backend = new AssetBundleBackend(bundlename);
        AssetBundleLookup();
        float progress = 0f;
        float progressInterval = 0f;
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
        progressInterval = 1f / lines.Length;
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
        CreateRustDirectory();
        SavePrefabsToAsset();
        MapIO.ClearProgressBar();
        mapIO.GetProjectPrefabs(); // Adds the prefabs just saved to the mapIO lookup.
        prefabsLoaded = true;
    }
    public void Dispose()
	{
		if (!isLoaded)
		{
			throw new System.Exception("Cannot unload assets before fully loaded!");
		}
        prefabsLoaded = false;
		backend.Dispose();
		backend = null;
	}
    public void LoadPrefabs(string path)
    {
        var subpaths = backend.FindAll(path);
        GameObject[] prefabs = new GameObject[subpaths.Length];
        for (int i = 0; i < subpaths.Length; i++)
        {
            if (subpaths[i].Contains(".prefab") && subpaths[i].Contains(".item") == false)
            {
                CreatePrefabDirectory(subpaths[i]);
                prefabs[i] = backend.Load<GameObject>(subpaths[i]);
                PreparePrefab(prefabs[i], subpaths[i], lookup[subpaths[i]]);
            }
        }
    }
    public void CreatePrefabDirectory(string path)
    {
        string[] folders = path.Split('/');
        var folderDirectory = "Assets";
        for (int i = 1; i < folders.Length - 1; i++) // Filters out the prefab path and gets the folder it's in.
        {
            folderDirectory += ("/" + folders[i]);
        }
        Directory.CreateDirectory(folderDirectory);
    }
    public void CreateRustDirectory()
    {
        Directory.CreateDirectory("Assets/Rust/Meshes");
    }
    public void AssetBundleLookup() 
    {
        var assets = backend.FindAll("");
        assetsList.Clear();
        foreach (var asset in assets)
        {
            assetsList.Add(asset);
        }
    }
    public void AssetDump() // Dumps every asset found in the Rust bundle to a text file.
    {
        using (StreamWriter streamWriter = new StreamWriter("AssetDump.txt", false))
        {
            foreach (var item in assetsList)
            {
                streamWriter.WriteLine(item);
            }
        }
    }
    public void PrefabsLoadedDump()
    {
        using(StreamWriter streamWriter = new StreamWriter("PrefabsLoaded.txt", false))
        {
            foreach (var item in prefabsList)
            {
                streamWriter.WriteLine(item.Prefab.name + ":" + item.Path + ":" + item.RustID);
            }
        }
    }
    public void PreparePrefab(GameObject go, string path, uint rustid) // Seperates the prefab components and adds them to list.
    {
        var prefabPath = path.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        var prefabMeshes = go.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < prefabMeshes.Length; i++) // Add all the meshes to the list to save to the project later.
        {
            if (!meshes.Contains(prefabMeshes[i].sharedMesh) && prefabMeshes[i].sharedMesh != null && prefabMeshes[i].sharedMesh.name != "Quad" && prefabMeshes[i].sharedMesh.name != "Sphere")
            {
                meshes.Add(prefabMeshes[i].sharedMesh);
            }
        }
        var fbxAssetPath = assetsList.FirstOrDefault(stringToCheck => stringToCheck.Contains(prefabName + ".fbx"));
        if (fbxAssetPath != null)
        {
            var fbxAsset = backend.Load<GameObject>(fbxAssetPath);
            fbxAsset.SetActive(true);
            fbxAsset.tag = "LoadedPrefab";
            fbxAsset.name = prefabName;
            PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
            prefabDataHolder.prefabData = new WorldSerialization.PrefabData
            {
                id = rustid,
            };
            prefabsList.Add(new PrefabAttributes()
            {
                Prefab = fbxAsset,
                Path = path.Replace(".prefab", ".fbx"),
                RustID = rustid
            });
        }
    }
    public void SavePrefabsToAsset() // Strip all the assets and save them to the project.
    {
        AssetDatabase.StartAssetEditing();
        foreach (var mesh in meshes)
        {
            if (!File.Exists("Assets/Rust/Meshes/" + mesh.name + ".asset"))
            {
                AssetDatabase.RemoveObjectFromAsset(mesh);
                if (mesh.isReadable)
                {
                    AssetDatabase.CreateAsset(mesh, "Assets/Rust/Meshes/" + mesh.name + ".asset");
                }
                else
                {
                    Debug.Log(mesh.name + " is not readable.");
                }
            }
        }
        meshes.Clear();
        foreach (var prefab in prefabsList)
        {
            if (!File.Exists(prefab.Path))
            {
                AssetDatabase.RemoveObjectFromAsset(prefab.Prefab);
                PrefabUtility.SaveAsPrefabAsset(prefab.Prefab, prefab.Path);
            }
        }
        AssetDatabase.StopAssetEditing();
        prefabsList.Clear();
    }
}