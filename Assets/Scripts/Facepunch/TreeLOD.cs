using UnityEngine;

[ExecuteAlways]
public class TreeLOD : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
