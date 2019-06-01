using UnityEngine;

[ExecuteAlways]
public class WaterCullingVolume : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}