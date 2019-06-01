using UnityEngine;

[ExecuteAlways]
public class RadialSpawnPoint : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}