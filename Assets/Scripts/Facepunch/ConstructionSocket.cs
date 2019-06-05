using UnityEngine;

[ExecuteAlways]
public class ConstructionSocket : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
