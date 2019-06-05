using UnityEngine;

[ExecuteAlways]
public class ScientistSpawner : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
