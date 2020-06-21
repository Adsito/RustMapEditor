using UnityEngine;

public static class GameObjectExt
{
    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        foreach (var transform in go.GetComponentsInChildren<Transform>())
        {
            transform.gameObject.layer = layer;
        }
    }

    public static void SetTagRecursively(this GameObject go, string tag)
    {
        foreach (var transform in go.GetComponentsInChildren<Transform>())
        {
            transform.gameObject.tag = tag;
        }
    }
}