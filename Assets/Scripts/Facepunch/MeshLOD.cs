using UnityEngine;

[ExecuteAlways]
public class MeshLOD : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}