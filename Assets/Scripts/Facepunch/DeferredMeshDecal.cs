using UnityEngine;

[ExecuteAlways]
public class DeferredMeshDecal : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}