using UnityEngine;

[ExecuteAlways]
public class CullingVolume : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
