using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PrefabLookup : System.IDisposable
{
	private AssetBundleBackend backend;
	private HashLookup lookup;

	public Dictionary<uint, GameObject> prefabs = new Dictionary<uint, GameObject>();

    private static string manifestPath = "Assets/manifest.asset";
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
        backend = new AssetBundleBackend(bundlename);
        var lookupString = "";
        var manifest = backend.Load<GameManifest>(manifestPath);
        for (uint index = 0; (long)index < (long)manifest.pooledStrings.Length; ++index)
        {
            lookupString += "0," + manifest.pooledStrings[index].hash + "," + manifest.pooledStrings[index].str + "\n";
            streamWriter.WriteLine(manifest.pooledStrings[index].hash + "  :  " + manifest.pooledStrings[index].str);
        }
        lookup = new HashLookup(lookupString);

        var lines = File.ReadAllLines(assetsToLoadPath);
        
        foreach (var line in lines)
        {
            if (line.EndsWith("/") || line.EndsWith("\\"))
            {
                loadPrefabs(line);
            }
        }
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
                streamWriter2.WriteLine(prefabs[i].name + " : " + subpaths[i] + " : " + lookup[subpaths[i]]);
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
    public void createPrefab(GameObject go, Transform prefabParent, string name, uint rustid) // Creates prefab, setting the LOD Group and gathering the LOD's.
    {
        var prefabPos = new Vector3(0, 0, 0);
        var prefabRot = new Quaternion(0, 0, 0, 0);
        GameObject loadedPrefab = GameObject.Instantiate(go, prefabPos, prefabRot);
        var prefabPath = name.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        loadedPrefab.name = prefabName;
        loadedPrefab.transform.SetParent(prefabParent);
        
        if (loadedPrefab.GetComponentsInChildren<LODGroup>() != null)
        {
            foreach (var group in loadedPrefab.GetComponentsInChildren<LODGroup>())
            {
                GameObject.DestroyImmediate(group);
            }
        }
        LODGroup lodGroup = loadedPrefab.AddComponent<LODGroup>(); //Setting LODGroup.
        PrefabDataHolder prefabDataHolder = loadedPrefab.AddComponent<PrefabDataHolder>();
        prefabDataHolder.saveWithMap = false;
        var lodMeshes = loadedPrefab.GetComponentsInChildren<MeshFilter>();
        MeshRenderer[][] renderers = new MeshRenderer[6][];
        int[] lodCount = new int[6];
        for (int i = 0; i < 6; i++)
        {
            renderers[i] = new MeshRenderer[lodMeshes.GetLength(0) + 1];
        }
        
        int lodNumber = 0;
        for (int i = 0; i < lodMeshes.Length; i++)
        {
            bool counted = false;
            if (lodMeshes[i].sharedMesh != null)
            {
                for (int j = 0; j < 6; j++) // Sorts out normal meshes into LOD groups
                {
                    if (lodMeshes[i].sharedMesh.name.EndsWith(j.ToString()))
                    {
                        lodCount[j]++;
                        lodNumber = j;
                        counted = true;
                        break;
                    }
                }
                if (lodMeshes[i].gameObject.GetComponent<MeshRenderer>() != null)
                {
                    foreach (Material item in lodMeshes[i].gameObject.GetComponent<MeshRenderer>().sharedMaterials)
                    {
                        if (item != null)
                        {
                            #region SetShaders
                            switch (item.shader.name)
                            {
                                case "Standard":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Standard (Specular setup)":
                                    item.shader = Shader.Find("Standard (Specular setup)");
                                    break;
                                case "Rust/Standard":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard (Specular setup)":
                                    item.shader = Shader.Find("Standard (Specular setup)");
                                    break;
                                case "Core/Foliage":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Core/Generic":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard Terrain Blend (Specular setup)":
                                    item.shader = Shader.Find("Standard (Specular setup)");
                                    break;
                                case "Core/Foliage Billboard":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard Blend 4-Way":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard Blend 4-Way (Specular setup)":
                                    item.shader = Shader.Find("Standard (Specular setup)");
                                    break;
                                case "Rust/Standard Blend Layer":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard Blend Layer (Specular setup)":
                                    item.shader = Shader.Find("Standard (Specular setup)");
                                    break;
                                case "Rust/Standard Decal":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard Decal (Specular setup)":
                                    item.shader = Shader.Find("Standard (Specular setup)");
                                    break;
                                case "Nature/Foliage":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Nature/Water/Lake":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Nature/Floating Ice Sheets":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard Cloth":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard + Wind":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Rust/Standard + Wind (Specular setup)":
                                    item.shader = Shader.Find("Standard(Specular setup)");
                                    break;
                                case "Rust/Standard Cloth (Specular setup)":
                                    item.shader = Shader.Find("Standard (Specular setup)");
                                    break;
                                case "Rust/Flare":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Custom/FoliageDisplace":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Custom/StandardWithDecal":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                case "Custom/Standard Refraction":
                                    item.shader = Shader.Find("Standard");
                                    break;
                                default:
                                    //Debug.Log("Shader: " + item.shader.name);
                                    break;
                            }
                            #endregion
                            if (!lodMeshes[i].sharedMesh.name.Contains("occluder"))
                            {
                                lodMeshes[i].gameObject.GetComponent<MeshRenderer>().enabled = true;
                            }
                            else
                            {
                                lodMeshes[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
                            }
                        }
                        else
                        {
                            lodMeshes[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                    if (counted == true)
                    {
                        renderers[lodNumber][lodCount[lodNumber]] = lodMeshes[i].gameObject.GetComponent<MeshRenderer>();
                    }
                    else // If its not a default naming scheme LOD we just add it to LOD0
                    {
                        lodCount[0]++;
                        renderers[0][lodCount[0]] = lodMeshes[i].gameObject.GetComponent<MeshRenderer>();
                    }
                }
            }
        }
        for (int i = 0; i < 6; i++) // Gets highest LOD number in object.
        {
            if (lodCount[i] == 0)
            {
                lodNumber = i;
                break;
            }
        }
        LOD[] lods = new LOD[lodNumber];
        for (int i = 0; i < lodNumber; i++)
        {
            lods[i] = new LOD(1.0F / (i + 1.5f), renderers[i]);
            if (lodNumber > 0)
            {
                lods[lodNumber - 1] = new LOD(0.175f, renderers[lodNumber - 1]);
            }
        }
        lodGroup.SetLODs(lods);
        lodGroup.fadeMode = LODFadeMode.SpeedTree;
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
