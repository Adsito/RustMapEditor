using UnityEngine;
using UnityEditor;
using static WorldSerialization;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

public static class PrefabManager
{
    public static GameObject DefaultPrefab { get; private set; }
    public static Transform PrefabParent { get; private set; }
    public static GameObject PrefabToSpawn;

    public static PrefabDataHolder[] CurrentMapPrefabs { get => PrefabParent.gameObject.GetComponentsInChildren<PrefabDataHolder>(); }

    public static Dictionary<string, Transform> PrefabCategories = new Dictionary<string, Transform>();

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
            if (!AssetManager.IsInitialised && SettingsManager.LoadBundleOnLaunch)
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
        newObj.transform.position = transform.position;
        newObj.transform.rotation = transform.rotation;
        newObj.transform.localScale = transform.localScale;
        newObj.name = name;
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

    public static void RenamePrefabs(PrefabDataHolder[] prefabs, string name)
    {
        foreach (var item in prefabs)
            item.prefabData.category = name;
    }

    public static void BreakPrefab(GameObject prefab)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(Coroutines.BreakPrefab(prefab));
    }

    private static class Coroutines
    {
        public static IEnumerator BreakPrefab(GameObject prefab)
        {
            for (int i = 0; i < Progress.GetCount(); i++) // Remove old progress
            {
                var progress = Progress.GetProgressById(Progress.GetId(i));
                if (progress.finished && progress.name.Contains("Break Prefab"))
                    progress.Remove();
            }

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var transforms = prefab.GetComponentsInChildren<Transform>();
            int progressId = Progress.Start("Break Prefab", null, Progress.Options.Sticky);

            var assetPaths = AssetManager.ManifestStrings.Where(x => x.Contains(".prefab") && !x.Contains(@"/ui/") && !x.Contains(@"assets/prefabs/building/") && !x.Contains(@"/engine/")
            && !x.Contains(@"/v2_rockformation_underwater/") && !x.Contains(@"/radtown work prefabs/") && !x.Contains(@"/system/") && AssetManager.ToID(x) != 0
            && !x.Contains(@"/test/") && AssetManager.ToID(x) != 1388803385).ToList();

            for (int i = 0; i < transforms.Length; i++)
            {
                if (sw.Elapsed.TotalSeconds > 0.2f)
                {
                    yield return null;
                    Progress.Report(progressId, (float)i / transforms.Length, "Scanning Prefab: " + i + " / " + transforms.Length);
                    sw.Restart();
                }
                var nameSplit = transforms[i].gameObject.name.Split(' ');
                var prefabName = nameSplit[0].Trim() + ".prefab";
                var prefabNameCheck = prefabName.ToLower();
                // cbf making another text file to read from.
                if (prefabNameCheck == "props.prefab" || prefabNameCheck == "lights.prefab" || prefabNameCheck == "beam.prefab" ||
                    prefabNameCheck == "on.prefab" || prefabNameCheck == "fur.prefab" || prefabNameCheck == "ore.prefab" || prefabNameCheck == "buildings.prefab")
                    continue;

                var prefabPaths = assetPaths.Where(x => x.Contains(prefabName));
                if (prefabPaths.Count() == 1)
                    Spawn(Load(prefabPaths.First()), transforms[i], prefabName);

                else if (prefabPaths.Count() > 1)
                {
                    foreach (var item in prefabPaths)
                    {
                        if (item.EndsWith(prefabName))
                        {
                            Spawn(Load(item), transforms[i], prefabName);
                            break;
                        }
                    }
                }
                else
                {
                    prefabName = prefabName.ToLower();

                    var prefabPathsLwr = assetPaths.Where(x => x.Contains(prefabName));
                    if (prefabPathsLwr.Count() == 1)
                        Spawn(Load(prefabPathsLwr.First()), transforms[i], prefabName);

                    else if (prefabPathsLwr.Count() > 1)
                    {
                        foreach (var item in prefabPathsLwr)
                        {
                            if (item.EndsWith(prefabName))
                            {
                                Spawn(Load(item), transforms[i], prefabName);
                                break;
                            }
                        }
                    }
                }
            }
            GameObject.DestroyImmediate(prefab);
            Progress.Report(progressId, 0.99f, "Scanned: " + transforms.Length + " prefabs.");
            Progress.Finish(progressId);
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
                Spawn(Load(prefabs[i].id), prefabs[i], GetParent(prefabs[i].category));
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
                prefabs[i].UpdatePrefabData();
                Spawn(DefaultPrefab, prefabs[i].prefabData, GetParent(prefabs[i].prefabData.category));
                GameObject.DestroyImmediate(prefabs[i].gameObject);
            }
            Progress.Report(progressID, 0.99f, "Replaced " + prefabs.Length + " prefabs.");
            Progress.Finish(progressID, Progress.Status.Succeeded);

            IsChangingPrefabs = false;
        }
    }
}