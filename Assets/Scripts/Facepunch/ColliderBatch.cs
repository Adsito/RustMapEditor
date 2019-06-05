using UnityEngine;

[ExecuteAlways]
public class ColliderBatch : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
