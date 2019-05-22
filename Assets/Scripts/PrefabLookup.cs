using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PrefabLookup : System.IDisposable
{
	private AssetBundleBackend backend;
	private HashLookup lookup;

	public Dictionary<uint, GameObject> prefabs = new Dictionary<uint, GameObject>();

    private static string manifestPath = "assets/manifest.asset";
    private static string assetsToLoadPath = "AssetsList.txt";
    public bool prefabsLoaded = false;

    public bool isLoaded
	{
		get { return prefabsLoaded; }
	}
    StreamWriter streamWriter = new StreamWriter("PrefabsManifest.txt", false);
    StreamWriter streamWriter2 = new StreamWriter("PrefabsLoaded.txt", false);

    public PrefabLookup(string bundlename, MapIO mapIO)
    {
        float progress = 0f;
        backend = new AssetBundleBackend(bundlename);
        var lookupString = "";
        var manifest = backend.Load<GameManifest>(manifestPath);
        if (manifest == null)
        {
            Debug.LogError("manifest is null");
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
        foreach (var line in lines)
        {
            mapIO.ProgressBar("Prefab Warmup", "Loading: " + line, progress);
            progress += progress;
            if (line.EndsWith("/") || line.EndsWith("\\"))
            {
                loadPrefabs(line);
            }
        }
        mapIO.ClearProgressBar();
        assetDump();
        streamWriter.Close();
        streamWriter2.Close();
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
    public void loadPrefabs(string path)
    {
        string[] subpaths = backend.FindAll(path);
        GameObject[] prefabs = new GameObject[subpaths.Length];
        Transform prefabParent = GameObject.Find("PrefabsLoaded").transform;
        foreach (var item in prefabParent.GetComponentsInChildren<MeshFilter>())
        {
            if (item.gameObject != null)
            {
                GameObject.DestroyImmediate(item.gameObject);
            }
        }
        for (int i = 0; i < subpaths.Length; i++)
        {
            if (subpaths[i].Contains(".prefab") && subpaths[i].Contains(".item") == false)
            {
                prefabs[i] = backend.LoadPrefab(subpaths[i]);
                createPrefab(prefabs[i], prefabParent, subpaths[i], lookup[subpaths[i]]);
                streamWriter2.WriteLine(prefabs[i].name + ":" + subpaths[i] + ":" + lookup[subpaths[i]]);
            }
        }
    }
    public void assetDump() // Dumps every asset found in the Rust bundle to a text file.
    {
        StreamWriter streamWriter3 = new StreamWriter("AssetDump.txt", false);
        var assetDump = backend.FindAll("");
        foreach (var item in assetDump)
        {
            streamWriter3.WriteLine(item);
        }
        streamWriter3.Close();
    }
    public void createPrefab(GameObject go, Transform prefabParent, string path, uint rustid) // Creates prefab, setting the LOD Group and gathering the LOD's.
    {
        var prefabPath = path.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        GameObject loadedPrefab = go;
        loadedPrefab.tag = "LoadedPrefab";
        PrefabDataHolder prefabDataHolder = loadedPrefab.AddComponent<PrefabDataHolder>();
        loadedPrefab.transform.parent = prefabParent;
        prefabDataHolder.prefabData = new WorldSerialization.PrefabData();
        var meshRenderers = loadedPrefab.GetComponentsInChildren<MeshRenderer>();
        MeshRenderer[] meshRenderer = new MeshRenderer[meshRenderers.Length + 1];
        MeshFilter[] meshFilters = new MeshFilter[meshRenderers.Length];
        CombineInstance[] combine = new CombineInstance[meshRenderers.Length];
        MeshFilter meshFilter = new MeshFilter();
        bool combineMeshes = false;
        if (loadedPrefab.GetComponent<MeshFilter>() == null)
        {
            meshFilter = loadedPrefab.AddComponent<MeshFilter>();
            combineMeshes = true;
        }
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].enabled == true)
            {
                if (meshRenderers[i].gameObject.name.Contains("occluder"))
                {
                    meshRenderers[i].enabled = false;
                }
                else
                {
                    if (meshRenderers[i].gameObject.GetComponent<MeshFilter>() != null)
                    {
                        meshRenderer[i] = meshRenderers[i];
                        combine[i].mesh = meshRenderer[i].gameObject.GetComponent<MeshFilter>().sharedMesh;
                        combine[i].transform = meshRenderer[i].transform.localToWorldMatrix;
                    }
                }
            }
        }
        if (combineMeshes)
        {
            meshFilter.mesh = new Mesh();
            meshFilter.mesh.CombineMeshes(combine, true);
        }
        if (loadedPrefab.GetComponent<LODGroup>())
        {
            GameObject.DestroyImmediate(loadedPrefab.GetComponent<LODGroup>(), true);
        }
        LODGroup lodGroup = loadedPrefab.AddComponent<LODGroup>();
        LOD[] lods = new LOD[1];
        lods[0] = new LOD(0.25f, meshRenderer);
        lodGroup.SetLODs(lods);
        lodGroup.fadeMode = LODFadeMode.None;
        lodGroup.RecalculateBounds();
        prefabs.Add(rustid, loadedPrefab);
        loadedPrefab.SetActive(false);
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
