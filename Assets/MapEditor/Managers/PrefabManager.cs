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
            return AssetManager.LoadPrefab(path);
        return DefaultPrefab;
    }

    /// <summary>Loads, sets up and returns the prefab at the prefab id.</summary>
    /// <param name="id">The prefab manifest id.</param>
    public static GameObject Load(uint id)
    {
        return Load(AssetManager.ToPath(id));
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
        prefabDataHolder.Setup();
        return go;
    }

    public static void Spawn(GameObject go, PrefabData prefabData, Transform parent)
    {
        GameObject newObj = GameObject.Instantiate(go, parent);
        newObj.transform.position = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z) + TerrainManager.MapOffset;
        newObj.transform.rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        newObj.transform.localScale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        newObj.name = go.name;
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
        newObj.SetActive(true);
    }

    /// <summary>Spawns the prefab set in PrefabToSpawn at the spawnPos</summary>
    public static void Spawn(Vector3 spawnPos)
    {
        if (PrefabToSpawn != null)
        {
            GameObject.Instantiate(PrefabToSpawn, spawnPos, Quaternion.Euler(0, 0, 0), PrefabParent).SetActive(true);
            PrefabToSpawn = null;
        }
    }

    /// <summary>Spawns prefabs for map load.</summary>
    public static void Spawn(PrefabData[] prefabs)
    {
        if (!Coroutines.IsBusy)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SpawnPrefabs(prefabs));
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

    private static class Coroutines
    {
        public static bool IsBusy { get; private set; }

        public static IEnumerator SpawnPrefabs(PrefabData[] prefabs)
        {
            IsBusy = true;
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(SpawnPrefabsCoroutine(prefabs));
            IsBusy = false;
        }

        public static IEnumerator ReplaceWithLoaded(PrefabDataHolder[] prefabs)
        {
            IsBusy = true;
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(ReplaceWithLoadedCoroutine(prefabs));
            IsBusy = false;
        }

        public static IEnumerator ReplaceWithDefault(PrefabDataHolder[] prefabs)
        {
            IsBusy = true;
            yield return EditorCoroutineUtility.StartCoroutineOwnerless(ReplaceWithDefaultCoroutine(prefabs));
            IsBusy = false;
        }

        private static IEnumerator SpawnPrefabsCoroutine(PrefabData[] prefabs)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ProgressBarManager.SetProgressIncrement(1f / prefabs.Length);
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 2f)
                {
                    sw.Restart();
                    ProgressBarManager.DisplayIncremental("Spawning Prefabs", "Spawning Prefabs: " + i + " / " + prefabs.Length);
                    yield return null;
                }
                else
                    ProgressBarManager.AddIncrement();

                Spawn(Load(prefabs[i].id), prefabs[i], PrefabParent);
            }
            ProgressBarManager.Clear();
        }

        private static IEnumerator ReplaceWithLoadedCoroutine(PrefabDataHolder[] prefabs)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            
            ProgressBarManager.SetProgressIncrement(1f / prefabs.Length);
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 2f)
                {
                    ProgressBarManager.DisplayIncremental("Replacing Prefabs", "Spawning Prefabs: " + i + " / " + prefabs.Length);
                    yield return null;
                    sw.Restart();
                }
                else
                    ProgressBarManager.AddIncrement();

                Spawn(Load(prefabs[i].prefabData.id), prefabs[i].prefabData, PrefabParent);
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            ProgressBarManager.Clear();
        }

        private static IEnumerator ReplaceWithDefaultCoroutine(PrefabDataHolder[] prefabs)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ProgressBarManager.SetProgressIncrement(1f / prefabs.Length);
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.05f)
                {
                    sw.Restart();
                    ProgressBarManager.DisplayIncremental("Replacing Prefabs", "Spawning Prefabs: " + i + " / " + prefabs.Length);
                    yield return null;
                }
                else
                    ProgressBarManager.AddIncrement();

                Spawn(DefaultPrefab, prefabs[i].prefabData, PrefabParent);
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            ProgressBarManager.Clear();
        }
    }
}