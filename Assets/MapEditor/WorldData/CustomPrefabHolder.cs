using System.Collections.Generic;
using UnityEngine;

[SelectionBase, DisallowMultipleComponent, ExecuteAlways]
public class CustomPrefabHolder : MonoBehaviour
{
    public CustomPrefab CustomPrefab = new CustomPrefab();

    public void Setup(string name, PrefabDataHolder[] prefabs)
    {
        var prefabsList = new List<CustomPrefab.PrefabData>();
        foreach (var item in prefabs)
            prefabsList.Add(new CustomPrefab.PrefabData { Prefab = item.prefabData });

        CustomPrefab.Name = name;
        CustomPrefab.Prefabs = prefabsList;
    }

    public void Update()
    {
        if (gameObject.transform.childCount == 0)
            GameObject.DestroyImmediate(gameObject);
    }
}