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
    public List<Texture2D> textures = new List<Texture2D>();
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
        CreateRustDirectory();
        float progress = 0f;
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
        progress = 1f / lines.Length;
        foreach (var line in lines)
        {
            mapIO.ProgressBar("Prefab Warmup", "Loading Directory: " + line, progress);
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
        if (!AssetDatabase.IsValidFolder("Assets/Rust/Meshes"))
        {
            AssetDatabase.CreateFolder("Assets/Rust", "Meshes");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Rust/Textures"))
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
        var prefabToSave = GameObject.Instantiate(go);
        var prefabRenderers = prefabToSave.GetComponentsInChildren<MeshRenderer>();
        var prefabMeshes = prefabToSave.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < prefabRenderers.Length; i++) // Add all the materials and shaders to the list to save to the project later.
        {
            var prefabMaterials = prefabRenderers[i].sharedMaterials;
            for (int j = 0; j < prefabMaterials.Length; j++)
            {
                if (!materials.Contains(prefabMaterials[j]))
                {
                    materials.Add(prefabMaterials[j]);
                }
            }
            foreach (Object tex in EditorUtility.CollectDependencies(prefabMaterials))
            {
                if (tex is Texture)
                {
                    if (!textures.Contains(tex))
                    {
                        textures.Add(tex as Texture2D);
                    }
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
        foreach (var texture in textures)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, tmp);
            RenderTexture.active = tmp;
            Texture2D newTexture = new Texture2D(texture.width, texture.height);
            newTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            newTexture.Apply();
            RenderTexture.ReleaseTemporary(tmp);
            File.WriteAllBytes("Assets/Rust/Textures/" + texture.name + ".tga", newTexture.EncodeToTGA());
            Resources.UnloadAsset(texture);
            newTexture = null;
        }
        textures.Clear();
        AssetDatabase.Refresh();
        AssetDatabase.StartAssetEditing();
        for (int i = 0; i < materials.Count; i++)
        {
            AssetDatabase.RemoveObjectFromAsset(materials[i]);
            if (materials[i].mainTexture != null && materials[i].HasProperty("_MainTex"))
            {
                var texture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Rust/Textures/" + materials[i].mainTexture.name + ".tga", typeof(Texture));
                if (texture != null)
                {
                    materials[i].mainTexture = texture;
                }
            }
            materials[i].shader = Shader.Find("Standard");
            AssetDatabase.CreateAsset(materials[i], "Assets/Rust/Materials/" + materials[i].name + ".mat");
        }
        materials.Clear();
        foreach (var mesh in meshes)
        {
            AssetDatabase.RemoveObjectFromAsset(mesh);
            AssetDatabase.CreateAsset(mesh, "Assets/Rust/Meshes/" + mesh.name + ".asset");
        }
        meshes.Clear();
        foreach (var prefab in prefabsList)
        {
            AssetDatabase.RemoveObjectFromAsset(prefab.Prefab);
            prefab.Prefab.SetActive(true);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab.Prefab);
            PrefabUtility.SaveAsPrefabAsset(prefab.Prefab, prefab.Path);
            GameObject.DestroyImmediate(prefab.Prefab);
        }
        prefabsList.Clear();
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
