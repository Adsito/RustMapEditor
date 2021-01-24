using UnityEngine;

public static class GameObjectExt
{
    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        foreach (var transform in go.GetComponentsInChildren<Transform>())
            transform.gameObject.layer = layer;
    }

    public static void SetTagRecursively(this GameObject go, string tag)
    {
        foreach (var transform in go.GetComponentsInChildren<Transform>())
            transform.gameObject.tag = tag;
    }

    public static void SetStaticRecursively(this GameObject go, bool active)
    {
        foreach (var transform in go.GetComponentsInChildren<Transform>(true))
            transform.gameObject.isStatic = active;
    }

    public static void RemoveNameUnderscore(this GameObject go)
    {
        go.name = go.name.Replace('_', ' ');
    }
}