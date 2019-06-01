using UnityEngine;

[ExecuteAlways]
public class CollectibleEntity : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
