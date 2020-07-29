using UnityEngine;
using UnityEditor;
using RustMapEditor.Data;
using static WorldSerialization;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Linq;

public static class PrefabManager
{
    public static GameObject DefaultPrefab { get; private set; }
    public static Transform PrefabParent { get; private set; }
    public static GameObject PrefabToSpawn;
    public static PrefabDataHolder[] CurrentMapPrefabs { get => PrefabParent.gameObject.GetComponentsInChildren<PrefabDataHolder>(); }

    public static bool IsChangingPrefabs { get; private set; }

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.update += OnProjectLoad;
    }

    private static void OnProjectLoad()
    {
        DefaultPrefab = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        PrefabParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        if (DefaultPrefab != null && PrefabParent != null)
        {
            EditorApplication.update -= OnProjectLoad;
            if (!AssetManager.IsInitialised && SettingsManager.LoadBundleOnProjectLoad)
                AssetManager.Initialise(SettingsManager.RustDirectory + SettingsManager.BundlePathExt);
        }
            
    }

    /// <summary>Loads, sets up and returns the prefab at the asset path.</summary>
    /// <param name="path">The prefab path in the bundle file.</param>
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
    public static GameObject Setup(GameObject go, string filePath)
    {
        go.SetLayerRecursively(8);
        go.SetTagRecursively("Untagged");
        go.RemoveNameUnderscore();
        foreach (var item in go.GetComponentsInChildren<MeshCollider>())
        {
            item.cookingOptions = MeshColliderCookingOptions.None;
            item.enabled = false;
            item.isTrigger = false;
            item.convex = false;
        }
        foreach (var item in go.GetComponentsInChildren<Animator>())
        {
            item.enabled = false;
            item.runtimeAnimatorController = null;
        }
        foreach (var item in go.GetComponentsInChildren<Light>())
            item.enabled = false;
        foreach (var item in go.GetComponentsInChildren<Canvas>())
            item.enabled = false;
        foreach (var item in go.GetComponentsInChildren<CanvasGroup>())
            item.enabled = false;

        PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new PrefabData() { id = AssetManager.ToID(filePath) };
        prefabDataHolder.Setup();
        return go;
    }

    /// <summary>Spawns a prefab, updates the PrefabData and parents to the selected transform.</summary>
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
    public static void SpawnPrefab(Vector3 spawnPos)
    {
        if (PrefabToSpawn != null)
        {
            GameObject newObj = GameObject.Instantiate(PrefabToSpawn, spawnPos, Quaternion.Euler(0, 0, 0), PrefabParent);
            newObj.name = PrefabToSpawn.name;
            newObj.SetActive(true);
            PrefabToSpawn = null;
        }
    }

    /// <summary>Spawns prefabs for map load.</summary>
    public static void SpawnPrefabs(PrefabData[] prefabs, int progressID)
    {
        if (!IsChangingPrefabs)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.SpawnPrefabs(prefabs, progressID));
    }

    /// <summary>Deletes prefabs from scene.</summary>
    public static void DeletePrefabs(PrefabDataHolder[] prefabs, int progressID = 0)
    {
        if (!IsChangingPrefabs)
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.DeletePrefabs(prefabs, progressID));
    }

    /// <summary>Replaces the selected prefabs with ones from the Rust bundles.</summary>
    public static void ReplaceWithLoaded(PrefabDataHolder[] prefabs, int progressID)
    {
        if (AssetManager.IsInitialised && !IsChangingPrefabs)
        {
            IsChangingPrefabs = true;
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.ReplaceWithLoaded(prefabs, progressID));
        }
    }

    /// <summary>Replaces the selected prefabs with the default prefabs.</summary>
    public static void ReplaceWithDefault(PrefabDataHolder[] prefabs, int progressID)
    {
        if (!IsChangingPrefabs)
        {
            IsChangingPrefabs = true;
            EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.ReplaceWithDefault(prefabs, progressID));
        }
    }

    public static void BreakPrefab(GameObject prefab)
    {
    }

    private static class Coroutines
    {
        {


        }

        public static IEnumerator SpawnPrefabs(PrefabData[] prefabs, int progressID)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 1.5f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Spawning Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                Spawn(Load(prefabs[i].id), prefabs[i], PrefabParent);
            }
            Progress.Report(progressID, 0.99f, "Spawned " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
        }

        public static IEnumerator DeletePrefabs(PrefabDataHolder[] prefabs, int progressID = 0)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            if (progressID == 0)
                progressID = Progress.Start("Delete Prefabs", null, Progress.Options.Sticky);

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.25f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Deleting Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Deleted " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
        }

        public static IEnumerator ReplaceWithLoaded(PrefabDataHolder[] prefabs, int progressID)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 1.5f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Replacing Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                Spawn(Load(prefabs[i].prefabData.id), prefabs[i].prefabData, PrefabParent);
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Replaced " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);

            IsChangingPrefabs = false;
        }

        public static IEnumerator ReplaceWithDefault(PrefabDataHolder[] prefabs, int progressID)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.05f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Replacing Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                Spawn(DefaultPrefab, prefabs[i].prefabData, PrefabParent);
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Replaced " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);

            IsChangingPrefabs = false;
        }
    }
}