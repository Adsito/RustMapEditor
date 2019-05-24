using UnityEngine;
using UnityEditor;
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
                LoadPrefabs(line);
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
    public void LoadPrefabs(string path)
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
        GameObject[] prefabs = new GameObject[subpaths.Length];
        for (int i = 0; i < subpaths.Length; i++)
        {
            if (subpaths[i].Contains(".prefab") && subpaths[i].Contains(".item") == false)
            {
                prefabs[i] = backend.LoadPrefab(subpaths[i]);
                streamWriter2.WriteLine(prefabs[i].name + ":" + subpaths[i] + ":" + lookup[subpaths[i]]);
                SavePrefabToAsset(prefabs[i], subpaths[i], lookup[subpaths[i]]);
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
    public void SavePrefabToAsset(GameObject go, string path, uint rustid) // Saves the prefab loaded from the game file into the project as an asset.
    {
        var prefabToSave = GameObject.Instantiate(go);
        var prefabPath = path.Split('/');
        var prefabName = prefabPath[prefabPath.Length - 1].Replace(".prefab", "");
        prefabToSave.tag = "LoadedPrefab";
        prefabToSave.name = prefabName;
        PrefabDataHolder prefabDataHolder = prefabToSave.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new WorldSerialization.PrefabData();
        prefabDataHolder.prefabData.id = rustid;
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefabToSave);
        PrefabUtility.SaveAsPrefabAsset(prefabToSave, path);
        //GameObject.DestroyImmediate(go, true);
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
