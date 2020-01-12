using UnityEngine;
using UnityEditor;
using RustMapEditor.Data;
using static WorldSerialization;

public static class PrefabManager
{
    public static GameObject defaultPrefab;

    [InitializeOnLoadMethod]
    public static void Init()
    {
        defaultPrefab = Resources.Load<GameObject>("Prefabs/DefaultPrefab");
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
    public static GameObject Prepare(GameObject go)
    {
        go.SetLayerRecursively(8);
        //go.SetActive(true);
        go.AddComponent<PrefabDataHolder>();
        return go;
    }
}