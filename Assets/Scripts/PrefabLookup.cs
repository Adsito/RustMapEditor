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
    public List<Texture> textures = new List<Texture>();
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
        SavePrefabsToAsset();
        mapIO.ClearProgressBar();
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
        prefabsList.Clear();
        string[] subpaths = backend.FindAll(path);
        CreatePrefabDirectory(path);
        GameObject[] prefabs = new GameObject[subpaths.Length];
        for (int i = 0; i < subpaths.Length; i++)
        {
            if (subpaths[i].Contains(".prefab") && subpaths[i].Contains(".item") == false)
            {
                prefabs[i] = backend.LoadPrefab(subpaths[i]);
                PreparePrefab(prefabs[i], subpaths[i], lookup[subpaths[i]]);
            }
        }
    }
    public void CreatePrefabDirectory(string path)
    {
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
    }
    public void CreateRustDirectory()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Rust"))
        {
            AssetDatabase.CreateFolder("Assets", "Rust");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Rust/Materials"))
        {
            AssetDatabase.CreateFolder("Assets/Rust", "Materials");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Rust"))
        {
            AssetDatabase.CreateFolder("Assets/Rust", "Meshes");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Rust"))
        {
            AssetDatabase.CreateFolder("Assets/Rust", "Textures");
        }
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
    public void PreparePrefab(GameObject go, string path, uint rustid) // Seperates the prefab components and adds them to list.
    {
        var prefabPath = path.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        var prefabToSave = GameObject.Instantiate(go);
        var prefabRenderers = prefabToSave.GetComponentsInChildren<MeshRenderer>();
        var prefabMeshes = prefabToSave.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < prefabRenderers.Length; i++) // Add all the materials and shaders to the list to save to the project later.
        {
            var prefabMaterials = prefabRenderers[i].sharedMaterials;
            for (int j = 0; j < prefabMaterials.Length; j++)
            {
                if (!materials.Contains(prefabMaterials[j]) && prefabMaterials[j] != null)
                {
                    materials.Add(prefabMaterials[j]);
                }
                if (!textures.Contains(prefabMaterials[j].mainTexture) && prefabMaterials[j].mainTexture != null)
                {
                    textures.Add(prefabMaterials[j].mainTexture);
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
    public void SavePrefabsToAsset() // Strip all the assets and save them to the project.
    {
        CreateRustDirectory();
        AssetDatabase.StartAssetEditing();
        /*
        foreach (var texture in textures)
        {
            AssetDatabase.RemoveObjectFromAsset(texture);
            AssetDatabase.CreateAsset(texture, "Assets/Rust/Textures/" + texture.name + ".tga");
        }*/
        foreach (var material in materials)
        {
            switch (material.shader.name)
            {
                case "Rust/Standard":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard (Specular setup)":
                    material.shader = Shader.Find("Standard (Specular setup)");
                    break;
                case "Core/Foliage":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Core/Generic":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard Terrain Blend (Specular setup)":
                    material.shader = Shader.Find("Standard (Specular setup)");
                    break;
                case "Core/Foliage Billboard":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard Blend 4-Way":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard Blend 4-Way (Specular setup)":
                    material.shader = Shader.Find("Standard (Specular setup)");
                    break;
                case "Rust/Standard Blend Layer":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard Blend Layer (Specular setup)":
                    material.shader = Shader.Find("Standard (Specular setup)");
                    break;
                case "Rust/Standard Decal":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard Decal (Specular setup)":
                    material.shader = Shader.Find("Standard (Specular setup)");
                    break;
                case "Nature/Foliage":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Nature/Water/Lake":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Nature/Floating Ice Sheets":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard Cloth":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard + Wind":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Rust/Standard + Wind (Specular setup)":
                    material.shader = Shader.Find("Standard(Specular setup)");
                    break;
                case "Rust/Standard Cloth (Specular setup)":
                    material.shader = Shader.Find("Standard (Specular setup)");
                    break;
                case "Rust/Flare":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Custom/FoliageDisplace":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Custom/StandardWithDecal":
                    material.shader = Shader.Find("Standard");
                    break;
                case "Custom/Standard Refraction":
                    material.shader = Shader.Find("Standard");
                    break;
            }
            AssetDatabase.RemoveObjectFromAsset(material);
            AssetDatabase.CreateAsset(material, "Assets/Rust/Materials/" + material.name + ".mat");
        }
        foreach (var mesh in meshes)
        {
            AssetDatabase.RemoveObjectFromAsset(mesh);
            AssetDatabase.CreateAsset(mesh, "Assets/Rust/Meshes/" + mesh.name + ".fbx");
        }
        foreach (var prefab in prefabsList)
        {
            AssetDatabase.RemoveObjectFromAsset(prefab.Prefab);
            prefab.Prefab.SetActive(true);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab.Prefab);
            PrefabUtility.SaveAsPrefabAsset(prefab.Prefab, prefab.Path);
            GameObject.DestroyImmediate(prefab.Prefab);
        }
        AssetDatabase.StopAssetEditing();
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
