using UnityEngine;

[ExecuteAlways]
public class SpawnGroup : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}