using UnityEngine;
using UnityEditor;
using RustMapEditor.Data;
using static WorldSerialization;

public static class PrefabManager
{
    public static GameObject defaultPrefab { get; private set; }
    public static Transform prefabParent { get; private set; }

    [InitializeOnLoadMethod]
    public static void Init()
    {
        EditorApplication.update += OnProjectLoad;
    }
    static void OnProjectLoad()
    {
        defaultPrefab = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
        prefabParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        EditorApplication.update -= OnProjectLoad;
    }
    /// <summary>Loads, sets up and returns the prefab at the asset path.</summary>
    /// <param name="path">The Prefab path in the bundle file.</param>
    public static GameObject Load(string path)
    {
        if (BundleManager.IsLoaded())
        {
            return BundleManager.Backend.LoadPrefab(path);
        }
        return defaultPrefab;
    }
    /// <summary>Loads, sets up and returns the prefab at the prefab id.</summary>
    /// <param name="id">The prefab manifest id.</param>
    public static GameObject Load(uint id)
    {
        return Load(StringPool.Get(id));
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
    /// <summary>Sets up the prefabs loaded from the bundle file for use in the editor.</summary>
    /// <param name="go">GameObject to process, should be from one of the asset bundles.</param>
    /// <param name="filePath">Asset filepath of the gameobject, used to get and set the PrefabID.</param>
    public static GameObject Process(GameObject go, string filePath)
    {
        go.SetActive(true);
        go.SetLayerRecursively(8);
        PrefabDataHolder prefabDataHolder = go.AddComponent<PrefabDataHolder>();
        prefabDataHolder.prefabData = new PrefabData() { id = StringPool.Get(filePath) };
        return go;
    }
}