using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class PrefabLookup : System.IDisposable
{
	private AssetBundleBackend backend;
	private HashLookup lookup;
	private Scene scene;

	private Dictionary<uint, GameObject> prefabs = new Dictionary<uint, GameObject>();

    private static string manifestPath = "Assets/manifest.asset";
    private static string assetsToLoadPath = "AssetsList.txt";
    public bool prefabsLoaded = false;

    public bool isLoaded
	{
		get { return prefabsLoaded; }
	}
    //StreamWriter streamWriter2 = new StreamWriter("PrefabsLoaded.txt", false);
    //StreamWriter streamWriter3 = new StreamWriter("PrefabsSpawned.txt", false);
    StreamWriter streamWriter4 = new StreamWriter("PrefabsMatched.txt", false);
    public PrefabLookup(string bundlename, MapIO mapIO)
    {
        backend = new AssetBundleBackend(bundlename);
        var lookupString = "";
        var manifest = backend.Load<GameManifest>(manifestPath);
        StreamWriter streamWriter = new StreamWriter("PrefabsManifest.txt", false);
        
        for (uint index = 0; (long)index < (long)manifest.pooledStrings.Length; ++index)
        {
            lookupString += "0," + manifest.pooledStrings[index].hash + "," + manifest.pooledStrings[index].str + "\n";
            streamWriter.WriteLine(manifest.pooledStrings[index].hash + "  :  " + manifest.pooledStrings[index].str);
        }
        lookup = new HashLookup(lookupString);
        streamWriter.Close();

        scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        Scene oldScene = SceneManager.GetActiveScene();
        
        SceneManager.SetActiveScene(scene);
        var lines = File.ReadAllLines(assetsToLoadPath);

        foreach (var line in lines)
        {
            if (line.EndsWith("/") || line.EndsWith("\\"))
            {
                loadPrefabs(line);
            }
        }
        //streamWriter2.Close();
        //streamWriter3.Close();
        streamWriter4.Close();

        SceneManager.SetActiveScene(oldScene);
        mapIO.loadAssetPrefabs();
        StaticBatchingUtility.Combine(GameObject.FindGameObjectsWithTag("LoadedPrefab"), GameObject.FindGameObjectWithTag("Prefabs"));
        prefabsLoaded = true;
    }

    public void Dispose()
	{
		if (!isLoaded)
		{
			throw new System.Exception("Cannot unload assets before fully loaded!");
		}
        foreach (GameObject prefab in scene.GetRootGameObjects())
        {
            GameObject.DestroyImmediate(prefab);
        }
		backend.Dispose();
		backend = null;
	}
    public void loadPrefabs(string path)
    {
        string[] subpaths = backend.FindAll(path);
        GameObject[] prefabs = new GameObject[subpaths.Length];
        //var prefabName = "";
        for (int i = 0; i < subpaths.Length; i++)
        {
            if (subpaths[i].Contains(".prefab"))
            {
                prefabs[i] = backend.LoadPrefab(subpaths[i]);
                createPrefab(prefabs[i], subpaths[i], lookup[subpaths[i]]);
                streamWriter4.WriteLine(prefabs[i].name + " : " + subpaths[i] + " : " + lookup[subpaths[i]]);
            }
            /*
            streamWriter2.WriteLine(subpaths[i]);
            for (int j = 0; j < prefabs.Length; j++)
            {
                //streamWriter3.WriteLine(prefabs[j].name);
                if (subpaths[i].Contains(prefabs[j].name + ".prefab"))
                {
                    prefabs[i] = backend.LoadPrefab(subpaths[i]);
                    //createPrefab(prefabs[i], subpaths[i], lookup[subpaths[i]]);
                    streamWriter4.WriteLine(prefabs[i].name + " : " + subpaths[i] + " : " + lookup[subpaths[i]]);
                }
                
                if (j > 0)
                {
                    if (subpaths[i].Contains(prefabs[j].name + ".prefab") && prefabs[j].name != prefabName)
                    {
                        prefabName = prefabs[j].name;
                        //createPrefab(prefabs[j], subpaths[i], lookup[subpaths[i]]);
                        streamWriter4.WriteLine(prefabs[j].name + " : " + subpaths[i] + " : " + lookup[subpaths[i]]);
                    }
                }
                else
                {
                    if (subpaths[i].Contains(prefabs[j].name + ".prefab"))
                    {
                        prefabs[i] = backend.LoadPrefab(subpaths[i]);
                        createPrefab(prefabs[i], subpaths[i], lookup[subpaths[i]]);
                        streamWriter4.WriteLine(prefabs[i].name + " : " + subpaths[i] + " : " + lookup[subpaths[i]]);
                    }
                }
            }
            */
        }
    }
    public void createPrefab(GameObject go, string name, uint rustid)
    {
        var prefabPos = new Vector3(0, 0, 0);
        var prefabRot = new Quaternion(0, 0, 0, 0);
        GameObject loadedPrefab = GameObject.Instantiate(go, prefabPos, prefabRot);
        loadedPrefab.name = name;
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
