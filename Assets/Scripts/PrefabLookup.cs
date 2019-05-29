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

	public Dictionary<uint, GameObject> prefabs = new Dictionary<uint, GameObject>();
    public List<Material> materials = new List<Material>();
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
    StreamWriter streamWriter = new StreamWriter("PrefabsManifest.txt", false);

    public PrefabLookup(string bundlename, MapIO mapIO)
    {
        float progress = 0f;
        backend = new AssetBundleBackend(bundlename);
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
                streamWriter.WriteLine(manifest.pooledStrings[index].hash + ":" + manifest.pooledStrings[index].str);
            }
        }
        lookup = new HashLookup(lookupString);
        var lines = File.ReadAllLines(assetsToLoadPath);
        float linesAmount = lines.Length;
        progress = 1f / linesAmount;
        AssetBundleLookup();
        foreach (var line in lines)
        {
            mapIO.ProgressBar("Prefab Warmup", "Loading: " + line, progress);
            progress += progress;
            if (line.EndsWith("/") || line.EndsWith("\\"))
            {
                LoadPrefabs(line);
            }
        }
        PrefabsLoadedDump();
        mapIO.ClearProgressBar();
        streamWriter.Close();
        prefabsLoaded = true;
    }
    public void Dispose()
	{
        LODGroup[] prefabsLoadedChildren = GameObject.Find("PrefabsLoaded").GetComponentsInChildren<LODGroup>(true);
		if (!isLoaded)
		{
			throw new System.Exception("Cannot unload assets before fully loaded!");
		}
        foreach (var prefab in prefabsLoadedChildren)
        {
            if (prefab.gameObject != null)
            {
                GameObject.DestroyImmediate(prefab.gameObject);
            }
        }
        prefabsLoaded = false;
		backend.Dispose();
		backend = null;
	}
    public void LoadPrefabs(string path)
    {
        assetsList.Clear();
        meshes.Clear();
        materials.Clear();
        string[] subpaths = backend.FindAll(path);
        foreach (var directory in subpaths) // Checks if directory exists in the project, if not creates one so we can save prefabs to it.
        {
            var folderDirectories = directory.Split('/');
            var currentDirectory = "Assets"; // Store the new directory on the depth it currently is.
            var oldDirectory = "";
            for (int i = 1; i < folderDirectories.Length - 1; i++) // Only get the folders from the split, not the prefab name.
            {
                oldDirectory = currentDirectory;
                currentDirectory += "/" + folderDirectories[i];
                if (!AssetDatabase.IsValidFolder(currentDirectory)) // Creates new folder in the project if it does not exist.
                {
                    AssetDatabase.CreateFolder(oldDirectory, folderDirectories[i]);
                }
            }
        }
        GameObject[] prefabs = new GameObject[subpaths.Length];
        for (int i = 0; i < subpaths.Length; i++)
        {
            if (subpaths[i].Contains(".prefab") && subpaths[i].Contains(".item") == false)
            {
                prefabs[i] = backend.LoadPrefab(subpaths[i]);
                PreparePrefab(prefabs[i], subpaths[i], lookup[subpaths[i]]);
            }
        }
        SavePrefabsToAsset();
    }
    public void AssetBundleLookup() // Dumps every asset found in the Rust bundle to a text file.
    {
        var assets = backend.FindAll("");
        assetsList.Clear();
        foreach (var asset in assets)
        {
            assetsList.Add(asset);
        }
    }
    public void AssetDump()
    {
        StreamWriter streamWriter = new StreamWriter("AssetDump.txt", false);
        foreach (var item in assetsList)
        {
            streamWriter.WriteLine(item);
        }
        streamWriter.Close();
    }
    public void PrefabsLoadedDump()
    {
        StreamWriter streamWriter = new StreamWriter("PrefabsLoaded.txt", false);
        foreach (var item in prefabsList)
        {
            streamWriter.WriteLine(item.Prefab.name + ":" + item.Path + ":" + item.RustID);
        }
        streamWriter.Close();
    }
    public void PreparePrefab(GameObject go, string path, uint rustid) // Saves the prefab loaded from the game file into the project as an asset.
    {
        var prefabPath = path.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        var prefabToSave = GameObject.Instantiate(go);
        var prefabRenderers = prefabToSave.GetComponentsInChildren<MeshRenderer>();
        var prefabMeshes = prefabToSave.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < prefabRenderers.Length; i++) // Add all the materials to the list to save to the project later.
        {
            var prefabMaterials = prefabRenderers[i].sharedMaterials;
            for (int j = 0; j < prefabMaterials.Length; j++)
            {
                if (!materials.Contains(prefabMaterials[j]) && prefabRenderers[i].sharedMaterials != null)
                {
                    materials.Add(prefabMaterials[j]);
                }
            }
        }
        for (int i = 0; i < prefabMeshes.Length; i++) // Add all the meshes to the list to save to the project later.
        {
            if (!meshes.Contains(prefabMeshes[i].sharedMesh) && prefabMeshes[i].sharedMesh != null)
            {
                meshes.Add(prefabMeshes[i].sharedMesh);
            }
        }
        prefabToSave.tag = "LoadedPrefab";
        prefabToSave.name = prefabName;
        PrefabDataHolder prefabDataHolder = prefabToSave.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new WorldSerialization.PrefabData();
        prefabDataHolder.prefabData.id = rustid;
        prefabsList.Add(new PrefabAttributes()
        {
            Prefab = prefabToSave,
            Path = path,
            RustID = rustid
        });
    }
    public void SavePrefabsToAsset()
    {
        foreach (var material in materials)
        {
            AssetDatabase.RemoveObjectFromAsset(material);
            AssetDatabase.CreateAsset(material, "Assets/Materials/Rust/Materials/" + material.name + ".mat");
        }
        foreach (var mesh in meshes)
        {
            AssetDatabase.RemoveObjectFromAsset(mesh);
            AssetDatabase.CreateAsset(mesh, "Assets/Materials/Rust/Meshes/" + mesh.name + ".fbx");
        }
        foreach (var prefab in prefabsList)
        {
            AssetDatabase.RemoveObjectFromAsset(prefab.Prefab);
            prefab.Prefab.SetActive(true);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab.Prefab);
            PrefabUtility.SaveAsPrefabAsset(prefab.Prefab, prefab.Path);
            GameObject.DestroyImmediate(prefab.Prefab);
        }
    }
    public GameObject this[uint uid]
	{
		get
		{
			GameObject res = null;

			if (!prefabs.TryGetValue(uid, out res))
			{
				throw new System.Exception("Prefab not found: " + uid + " - assets not fully loaded yet?");
			}
			return res;
		}
	}
}
