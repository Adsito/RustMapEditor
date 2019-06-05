using UnityEngine;

[ExecuteAlways]
public class AiLocationSpawner : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
