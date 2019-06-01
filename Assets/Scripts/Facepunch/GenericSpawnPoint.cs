using UnityEngine;

[ExecuteAlways]
public class GenericSpawnPoint : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}