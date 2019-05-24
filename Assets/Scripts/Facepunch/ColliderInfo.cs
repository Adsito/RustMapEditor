using UnityEngine;

[ExecuteAlways]
public class ColliderInfo : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
