using UnityEngine;

[ExecuteAlways]
public class PrefabParameters : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}