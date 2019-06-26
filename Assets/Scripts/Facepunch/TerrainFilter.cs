using UnityEngine;

[ExecuteAlways]
public class TerrainFilter : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}