using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PrefabLookup : System.IDisposable
{
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
        //var lookupManifest = backend.Load<GameManifest>(manifestPath);
        
        /*
        for (int i = 0; i < lookupManifest.prefabProperties.Length; i++)
        {
            Debug.Log(lookupManifest.prefabProperties[i].hash);
        } 
        Enable to turn on a list of all the prefabs in Rust.
        */
        

        lookup = new HashLookup(lookupAsset.text);

		var asyncOperation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

		scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

		asyncOperation.completed += (operation) =>
		{
			foreach (var go in scene.GetRootGameObjects())
			{
				prefabs.Add(lookup[go.name], go);
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
