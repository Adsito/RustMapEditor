using UnityEngine;

[ExecuteAlways]
public class TerrainAnchor : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}