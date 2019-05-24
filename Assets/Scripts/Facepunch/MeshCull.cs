using UnityEngine;

[ExecuteAlways]
public class MeshCull : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
