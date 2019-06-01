using UnityEngine;

[ExecuteAlways]
public class Poolable : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}