using UnityEngine;
using UnityEditor;
using RustMapEditor.Data;
using static WorldSerialization;
using Unity.EditorCoroutines.Editor;
using System.Collections;

public static class PrefabManager
{
    public static GameObject DefaultPrefab { get; private set; }
    public static Transform PrefabParent { get; private set; }
    public static GameObject PrefabToSpawn;

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.update += OnProjectLoad;
    }

    private static void OnProjectLoad()
    {
        DefaultPrefab = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        PrefabParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        EditorApplication.update -= OnProjectLoad;
    }

    /// <summary>Loads, sets up and returns the prefab at the asset path.</summary>
    /// <param name="path">The Prefab path in the bundle file.</param>
    public static GameObject Load(string path)
    {
        if (AssetManager.IsInitialised)
        {
            return AssetManager.LoadPrefab(path);
        }
        return DefaultPrefab;
    }

    /// <summary>Loads, sets up and returns the prefab at the prefab id.</summary>
    /// <param name="id">The prefab manifest id.</param>
    public static GameObject Load(uint id)
    {
        return Load(AssetManager.ToPath(id));
    }

    public static void Spawn(GameObject go, PrefabData prefabData, Transform parent)
    {
        GameObject newObj = GameObject.Instantiate(go, parent);
        newObj.transform.position = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z) + LandData.GetMapOffset();
        newObj.transform.rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        newObj.transform.localScale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        newObj.name = go.name;
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
    }

    /// <summary>Spawns the prefab set in PrefabToSpawn at the spawnPos</summary>
    public static void Spawn(Vector3 spawnPos)
    {
        if (PrefabToSpawn != null)
        {
            GameObject.Instantiate(PrefabToSpawn, spawnPos, Quaternion.Euler(0, 0, 0), PrefabParent);
            PrefabToSpawn = null;
        }
    }

    /// <summary>Sets up the prefabs loaded from the bundle file for use in the editor.</summary>
    /// <param name="go">GameObject to process, should be from one of the asset bundles.</param>
    /// <param name="filePath">Asset filepath of the gameobject, used to get and set the PrefabID.</param>
    public static GameObject Process(GameObject go, string filePath)
    {
        go.SetLayerRecursively(8);
        go.SetTagRecursively("Untagged");
        foreach (var item in go.GetComponentsInChildren<MeshCollider>())
        {
            item.cookingOptions = MeshColliderCookingOptions.None;
            item.isTrigger = false;
            item.convex = false;
        }
        PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new PrefabData() { id = AssetManager.ToID(filePath) };
        go.SetActive(true);
        return go;
    }

    /// <summary>Replaces the selected prefabs with ones from the Rust bundles.</summary>
    public static void ReplaceWithLoaded(PrefabDataHolder[] prefabs)
    {
        if (AssetManager.IsInitialised && !Coroutines.IsBusy)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.ReplaceWithLoaded(prefabs));
    }

    /// <summary>Replaces the selected prefabs with the default prefabs.</summary>
    public static void ReplaceWithDefault(PrefabDataHolder[] prefabs)
    {
        if (!Coroutines.IsBusy)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.ReplaceWithDefault(prefabs));
    }

    public static class Coroutines
    {
        public static bool IsBusy { get; private set; }

        public static IEnumerator ReplaceWithLoaded(PrefabDataHolder[] prefabs)
        {
            IsBusy = true;
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(ReplaceWithLoadedCoroutine(prefabs));
            IsBusy = false;
        }

        private static IEnumerator ReplaceWithLoadedCoroutine(PrefabDataHolder[] prefabs)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                MapManager.progressValue += 1f / prefabs.Length;
                if (sw.Elapsed.TotalSeconds > 0.05f)
                {
                    sw.Restart();
                    MapManager.ProgressBar("Preparing Prefabs", "Processing Prefabs: " + i + " / " + prefabs.Length, MapManager.progressValue);
                }
                PrefabManager.Load(prefabs[i].prefabData.id);
            }
            MapManager.ClearProgressBar();

            sw.Restart();
            for (int i = 0; i < prefabs.Length; i++)
            {
                MapManager.progressValue += 1f / prefabs.Length;
                if (sw.Elapsed.TotalSeconds > 0.1f)
                {
                    MapManager.ProgressBar("Replacing Prefabs", "Spawning Prefabs: " + i + " / " + prefabs.Length, MapManager.progressValue);
                    yield return null;
                    sw.Restart();
                }
                PrefabManager.Spawn(PrefabManager.Load(prefabs[i].prefabData.id), prefabs[i].prefabData, PrefabManager.PrefabParent);
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            MapManager.ClearProgressBar();
            yield return null;
        }

        public static IEnumerator ReplaceWithDefault(PrefabDataHolder[] prefabs)
        {
            IsBusy = true;
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(ReplaceWithDefaultCoroutine(prefabs));
            IsBusy = false;
        }

        private static IEnumerator ReplaceWithDefaultCoroutine(PrefabDataHolder[] prefabs)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < prefabs.Length; i++)
            {
                MapManager.progressValue += 1f / prefabs.Length;
                if (sw.Elapsed.TotalSeconds > 0.05f)
                {
                    sw.Restart();
                    MapManager.ProgressBar("Replacing Prefabs", "Spawning Prefabs: " + i + " / " + prefabs.Length, MapManager.progressValue);
                    yield return null;
                }
                Spawn(DefaultPrefab, prefabs[i].prefabData, PrefabParent);
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            MapManager.ClearProgressBar();
        }
    }
}