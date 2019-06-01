using UnityEngine;

[ExecuteAlways]
public class DeferredDecal : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}