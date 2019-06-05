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
            mapIO.ProgressBar("Prefab Warmup", "Loading Directory: " + line, progress);
            progress += progressInterval;
            if (line.EndsWith("/") || line.EndsWith("\\"))
            {
                LoadPrefabs(line);
            }
        }
        PrefabsLoadedDump();
        //SavePrefabsToAsset();
        mapIO.ClearProgressBar();
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
                prefabs[i] = backend.LoadPrefab(subpaths[i]);
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
        Directory.CreateDirectory("Assets/Rust/Materials");
        Directory.CreateDirectory("Assets/Rust/Meshes");
        Directory.CreateDirectory("Assets/Rust/Textures");
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
        var prefabMeshes = go.GetComponentsInChildren<MeshFilter>();
        var rectTransforms = go.GetComponentsInChildren<RectTransform>();
        if (rectTransforms.Length > 0) // Remove UI prefabs.
        {
            return;
        }
        /*
        var prefabRenderers = go.GetComponentsInChildren<MeshRenderer>();
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
                    var texture = tex as Texture;
                    if (!textures.Contains(texture) && texture.name.Contains("_diff"))
                    {
                        textures.Add(tex as Texture2D);
                    }
                }
            }
        }
        for (int i = 0; i < prefabMeshes.Length; i++) // Add all the meshes to the list to save to the project later.
        {
            if (!meshes.Contains(prefabMeshes[i].sharedMesh) && prefabMeshes[i].sharedMesh != null && prefabMeshes[i].sharedMesh.name != "Quad" && prefabMeshes[i].sharedMesh.name != "Sphere")
            {
                meshes.Add(prefabMeshes[i].sharedMesh);
            }
        }*/
        go.tag = "LoadedPrefab";
        go.name = prefabName;
        PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new WorldSerialization.PrefabData();
        prefabDataHolder.prefabData.id = rustid;
        var prefabChildren = go.GetComponentsInChildren<Transform>(true);
        foreach (var item in prefabChildren)
        {
            item.gameObject.SetActive(true);
        }
        GameObject newObj = GameObject.Instantiate(go);
        prefabsList.Add(new PrefabAttributes()
        {
            Prefab = go,
            Path = path,
            RustID = rustid
        });
    }
    public void SavePrefabsToAsset() // Strip all the assets and save them to the project.
    {
        foreach (var texture in textures)
        {
            if (!File.Exists("Assets/Rust/Textures/" + texture.name + ".tga"))
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
        }
        textures.Clear();
        AssetDatabase.Refresh();
        AssetDatabase.StartAssetEditing();
        for (int i = 0; i < materials.Count; i++)
        {
            if (!File.Exists("Assets/Rust/Materials/" + materials[i].name + ".mat"))
            {
                AssetDatabase.RemoveObjectFromAsset(materials[i]);
                if (materials[i].HasProperty("_MainTex"))
                {
                    var texture = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Rust/Textures/" + materials[i].mainTexture.name + ".tga", typeof(Texture));
                    if (texture != null)
                    {
                        materials[i].mainTexture = texture;
                    }
                }
                if (materials[i].shader.name.Contains("Specular"))
                {
                    materials[i].shader = Shader.Find("Standard (Specular setup)");
                }
                else
                {
                    materials[i].shader = Shader.Find("Standard");
                }
                AssetDatabase.CreateAsset(materials[i], "Assets/Rust/Materials/" + materials[i].name + ".mat");
            }
        }
        materials.Clear();
        foreach (var mesh in meshes)
        {
            if (!File.Exists("Assets/Rust/Meshes/" + mesh.name + ".asset"))
            {
                AssetDatabase.RemoveObjectFromAsset(mesh);
                AssetDatabase.CreateAsset(mesh, "Assets/Rust/Meshes/" + mesh.name + ".asset");
            }
        }
        meshes.Clear();
        foreach (var prefab in prefabsList)
        {
            if (!File.Exists(prefab.Path))
            {
                AssetDatabase.RemoveObjectFromAsset(prefab.Prefab);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab.Prefab);
                PrefabUtility.SaveAsPrefabAsset(prefab.Prefab, prefab.Path);
                GameObject.DestroyImmediate(prefab.Prefab);
            }
        }
        prefabsList.Clear();
        AssetDatabase.StopAssetEditing();
        Resources.UnloadUnusedAssets();
    }
}