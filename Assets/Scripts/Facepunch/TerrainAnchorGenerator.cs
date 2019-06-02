using UnityEngine;

[ExecuteAlways]
public class TerrainAnchorGenerator : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}