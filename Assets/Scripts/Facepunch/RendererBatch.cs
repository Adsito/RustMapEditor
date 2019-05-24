using UnityEngine;

[ExecuteAlways]
public class RendererBatch : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
