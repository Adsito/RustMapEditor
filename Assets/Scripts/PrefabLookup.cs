using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class PrefabLookup : System.IDisposable
{
    private AssetBundle bundle;
	private AssetBundleBackend backend;
	private HashLookup lookup;
	private Scene scene;

	private Dictionary<uint, GameObject> prefabs = new Dictionary<uint, GameObject>();

	private static string lookupPath = "Assets/Modding/Prefabs.txt";
	private static string scenePath = "Assets/Modding/Prefabs.unity";
    private static string manifestPath = "Assets/manifest.asset";

    public bool isLoaded
	{
		get { return scene.isLoaded; }
	}

    public PrefabLookup(string bundlename)
    {
        backend = new AssetBundleBackend(bundlename);
        var lookupAsset = backend.Load<TextAsset>(lookupPath);
        var manifest = backend.Load<GameManifest>(manifestPath);
        var prefabLoad = backend.Load<GameObject>("assets/bundled/prefabs/autospawn/monument/cave/cave_large_hard.prefab");
        var prefabObject = GameObject.Instantiate(prefabLoad);
        StreamWriter streamWriter = new StreamWriter(@"F:\Documents\GitHub\prefaboutput2.txt", false);


        
        for (int i = 0; i < manifest.prefabProperties.GetLength(0); i++)
        {
            if (manifest.prefabProperties[i].name.EndsWith(".prefab"))
            {
                
                if (manifest.prefabProperties[i].name.StartsWith("assets/bundled"))
                {
                    prefabLoad = backend.Load<GameObject>(manifest.prefabProperties[i].name);
                    prefabObject = GameObject.Instantiate(prefabLoad);
                    //prefabs.Add(manifest.prefabProperties[i].hash, prefabObject);
                    streamWriter.WriteLine(manifest.prefabProperties[i].name);
                    streamWriter.WriteLine(manifest.prefabProperties[i].hash);
                }
                else if (manifest.prefabProperties[i].name.StartsWith("assets/content"))
                {
                    prefabLoad = backend.Load<GameObject>(manifest.prefabProperties[i].name);
                    prefabObject = GameObject.Instantiate(prefabLoad);
                    //prefabs.Add(manifest.prefabProperties[i].hash, prefabObject);
                    streamWriter.WriteLine(manifest.prefabProperties[i].name);
                    streamWriter.WriteLine(manifest.prefabProperties[i].hash);
                }
                else if (manifest.prefabProperties[i].name.StartsWith("assets/prefabs/building core"))
                {
                    prefabLoad = backend.Load<GameObject>(manifest.prefabProperties[i].name);
                    prefabObject = GameObject.Instantiate(prefabLoad);
                    //prefabs.Add(manifest.prefabProperties[i].hash, prefabObject);
                    streamWriter.WriteLine(manifest.prefabProperties[i].name);
                    streamWriter.WriteLine(manifest.prefabProperties[i].hash);
                }
                
                else if (manifest.prefabProperties[i].name.StartsWith("assets/prefabs/deployable"))
                {
                    prefabLoad = backend.Load<GameObject>(manifest.prefabProperties[i].name);
                    prefabObject = GameObject.Instantiate(prefabLoad);
                    //prefabs.Add(manifest.prefabProperties[i].hash, prefabObject);
                    streamWriter.WriteLine(manifest.prefabProperties[i].name);
                    streamWriter.WriteLine(manifest.prefabProperties[i].hash);
                }
            }
        }
     
        streamWriter.Close();
        // Assign prefab LOD's to the white cubes in prefab loading.
        //
        //gameObject.name = "assets/prefabs/building core/wall.frame/wall.frame.metal.prefab";
        return;

        var lookup = new HashLookup(lookupAsset.text);
        var asyncOperation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

		scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

		asyncOperation.completed += (operation) =>
		{
			foreach (var go in scene.GetRootGameObjects())
			{
                prefabs.Add(lookup[go.name], go);
                // Add the other UIDs here.
            }
		};
    }

    public void Dispose()
	{
		if (!isLoaded)
		{
			throw new System.Exception("Cannot unload assets before fully loaded!");
		}

		backend.Dispose();
		backend = null;

		SceneManager.UnloadSceneAsync(scene);
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
