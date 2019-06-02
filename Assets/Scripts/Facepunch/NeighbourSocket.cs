using UnityEngine;

[ExecuteAlways]
public class NeighbourSocket : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}