using System.Collections.Generic;
using UnityEngine;

[SelectionBase, DisallowMultipleComponent, ExecuteAlways]
public class CustomPrefabHolder : MonoBehaviour
{
    public CustomPrefab CustomPrefab = new CustomPrefab();

    public void Setup(PrefabDataHolder[] prefabs)
    {
        var prefabsList = new List<CustomPrefab.PrefabData>();
        foreach (var item in prefabs)
            prefabsList.Add(new CustomPrefab.PrefabData { Prefab = item.prefabData });

        CustomPrefab.Prefabs = prefabsList;
    }

    public void Update()
    {
        if (gameObject.transform.childCount == 0)
            GameObject.DestroyImmediate(gameObject);
    }
}