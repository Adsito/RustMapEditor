﻿using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static WorldSerialization;

public static class PrefabManager
{
    #region Init
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
            EditorApplication.update -= OnProjectLoad;
    }
    #endregion

    public static class Callbacks
    {
        public delegate void PrefabManagerCallback(GameObject prefab);

        /// <summary>Called after prefab is loaded and setup from bundle. </summary>
        public static event PrefabManagerCallback PrefabLoaded;
        /// <summary>Called after prefab category is renamed.</summary>
        public static event PrefabManagerCallback PrefabCategoryChanged;
        /// <summary>Called after prefab ID is changed.</summary>
        public static event PrefabManagerCallback PrefabIDChanged;

        public static void OnPrefabLoaded(GameObject prefab) => PrefabLoaded?.Invoke(prefab);
        public static void OnPrefabCategoryChanged(GameObject prefab) => PrefabCategoryChanged?.Invoke(prefab);
        public static void OnPrefabIDChanged(GameObject prefab) => PrefabIDChanged?.Invoke(prefab);
    }

    public static GameObject DefaultPrefab { get; private set; }
    public static Transform PrefabParent { get; private set; }
    public static GameObject PrefabToSpawn;

    /// <summary>List of prefab names from the asset bundle.</summary>
    private static List<string> Prefabs;

    /// <summary>Prefabs currently spawned on the map.</summary>
    public static PrefabDataHolder[] CurrentMapPrefabs { get => PrefabParent.gameObject.GetComponentsInChildren<PrefabDataHolder>(); }

    public static Dictionary<string, Transform> PrefabCategories = new Dictionary<string, Transform>();

    public static bool IsChangingPrefabs { get; private set; }

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
    public static GameObject Load(uint id) => Load(AssetManager.ToPath(id));

    /// <summary>Searches through all prefabs found in bundle files, returning matches.</summary>
    /// <returns>List of strings containing the path matching the <paramref name="key"/>.</returns>
    public static List<string> Search(string key)
    {
        if (Prefabs == null)
        {
            Prefabs = new List<string>();
            foreach (var i in AssetManager.AssetPaths)
                if (i.EndsWith(".prefab"))
                    Prefabs.Add(i);

            Prefabs.OrderBy(x => x);
        }

        return Prefabs.Where(x => x.Contains(key)).ToList();
    }

    /// <summary>Gets the parent prefab category transform from the hierachy.</summary>
    public static Transform GetParent(string category)
    {
        if (PrefabCategories.TryGetValue(category, out Transform transform))
            return transform;

        var obj = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PrefabCategory"), PrefabParent, false);
        obj.transform.localPosition = Vector3.zero;
        obj.name = category;
        PrefabCategories.Add(category, obj.transform);
        return obj.transform;
    }

    /// <summary>Sets up the prefabs loaded from the bundle file for use in the editor.</summary>
    /// <param name="go">GameObject to process, should be from one of the asset bundles.</param>
    /// <param name="filePath">Asset filepath of the gameobject, used to get and set the PrefabID.</param>
    public static GameObject Setup(GameObject go, string filePath)
    {
        go.SetLayerRecursively(8);
        go.SetTagRecursively("Untagged");
        go.SetStaticRecursively(true);
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
        foreach (var item in go.GetComponentsInChildren<ParticleSystem>())
        {
            var emission = item.emission;
            emission.enabled = false;
        }

        PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new PrefabData() { id = AssetManager.ToID(filePath) };

        go.SetActive(false);
        return go;
    }

    /// <summary>Spawns a prefab, updates the PrefabData and parents to the selected transform.</summary>
    public static void Spawn(GameObject go, PrefabData prefabData, Transform parent)
    {
        GameObject newObj = GameObject.Instantiate(go, parent);
        newObj.transform.localPosition = new Vector3(prefabData.position.x, prefabData.position.y, prefabData.position.z);
        newObj.transform.rotation = Quaternion.Euler(new Vector3(prefabData.rotation.x, prefabData.rotation.y, prefabData.rotation.z));
        newObj.transform.localScale = new Vector3(prefabData.scale.x, prefabData.scale.y, prefabData.scale.z);
        newObj.name = go.name;
        newObj.GetComponent<PrefabDataHolder>().prefabData = prefabData;
        newObj.SetActive(true);
    }

    /// <summary>Spawns a prefab and parents to the selected transform.</summary>
    public static void Spawn(GameObject go, Transform transform, string name)
    {
        GameObject newObj = GameObject.Instantiate(go, PrefabParent);
        newObj.transform.SetPositionAndRotation(transform.position, transform.rotation);
        newObj.transform.localScale = transform.localScale;
        newObj.name = name;
        newObj.SetActive(true);
    }

    /// <summary>Spawns the prefab set in PrefabToSpawn at the spawnPos</summary>
    public static void SpawnPrefab(Vector3 spawnPos)
    {
        if (PrefabToSpawn != null)
        {
            GameObject newObj = GameObject.Instantiate(PrefabToSpawn, spawnPos, Quaternion.Euler(0, 0, 0));
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

    public static void RenamePrefabCategories(PrefabDataHolder[] prefabs, string name)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.RenamePrefabCategories(prefabs, name));
    }

    public static void RenamePrefabIDs(PrefabDataHolder[] prefabs, uint id, bool replace)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.RenamePrefabIDs(prefabs, id, replace));
    }

    /// <summary>Rotates all prefabs in map 90° Clockwise or Counter Clockwise.</summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotatePrefabs(bool CW)
    {
        PrefabParent.Rotate(0, 90, 0, Space.World);
        PrefabParent.gameObject.GetComponent<LockObject>().UpdateTransform();
    }

    private static class Coroutines
    {
        public static IEnumerator SpawnPrefabs(PrefabData[] prefabs, int progressID)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 4f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Spawning Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                Spawn(Load(prefabs[i].id), prefabs[i], GetParent(prefabs[i].category));
            }

            Progress.Report(progressID, 0.99f, "Spawned " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
        }

        public static IEnumerator DeletePrefabs(PrefabDataHolder[] prefabs, int progressID = 0)
        {
            var sw = new System.Diagnostics.Stopwatch();
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
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 4f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Replacing Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                prefabs[i].UpdatePrefabData();
                Spawn(Load(prefabs[i].prefabData.id), prefabs[i].prefabData, GetParent(prefabs[i].prefabData.category));
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }

            Progress.Report(progressID, 0.99f, "Replaced " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
            IsChangingPrefabs = false;
        }

        public static IEnumerator ReplaceWithDefault(PrefabDataHolder[] prefabs, int progressID)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.05f)
                {
                    yield return null;
                    Progress.Report(progressID, (float)i / prefabs.Length, "Replacing Prefabs: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
                prefabs[i].UpdatePrefabData();
                Spawn(DefaultPrefab, prefabs[i].prefabData, GetParent(prefabs[i].prefabData.category));
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }

            Progress.Report(progressID, 0.99f, "Replaced " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);
            IsChangingPrefabs = false;
        }

        public static IEnumerator RenamePrefabCategories(PrefabDataHolder[] prefabs, string name)
        {
            ProgressManager.RemoveProgressBars("Rename Prefab Categories");
            int progressId = Progress.Start("Rename Prefab Categories", null, Progress.Options.Sticky);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].prefabData.category = name;
                Callbacks.OnPrefabCategoryChanged(prefabs[i].gameObject);
                if (sw.Elapsed.TotalSeconds > 0.2f)
                {
                    yield return null;
                    Progress.Report(progressId, (float)i / prefabs.Length, "Renaming Prefab: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
            }

            Progress.Report(progressId, 0.99f, "Renamed: " + prefabs.Length + " prefabs.");
            Progress.Finish(progressId);
        }

        public static IEnumerator RenamePrefabIDs(PrefabDataHolder[] prefabs, uint id, bool replace)
        {
            ProgressManager.RemoveProgressBars("Rename Prefab IDs");
            int progressId = Progress.Start("Rename Prefab IDs", null, Progress.Options.Sticky);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < prefabs.Length; i++)
            {
                prefabs[i].prefabData.id = id;
                Callbacks.OnPrefabIDChanged(prefabs[i].gameObject);
                if (replace)
                {
                    prefabs[i].UpdatePrefabData();
                    Spawn(Load(prefabs[i].prefabData.id), prefabs[i].prefabData, GetParent(prefabs[i].prefabData.category));
                    GameObject.DestroyImmediate(prefabs[i].gameObject);
                }
                if (sw.Elapsed.TotalSeconds > 0.2f)
                {
                    yield return null;
                    Progress.Report(progressId, (float)i / prefabs.Length, "Renaming Prefab: " + i + " / " + prefabs.Length);
                    sw.Restart();
                }
            }

            Progress.Report(progressId, 0.99f, "Renamed: " + prefabs.Length + " prefabs.");
            Progress.Finish(progressId);
        }
    }
}