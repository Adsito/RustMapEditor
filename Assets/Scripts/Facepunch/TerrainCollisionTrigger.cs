using UnityEngine;

[ExecuteAlways]
public class TerrainCollisionTrigger : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}