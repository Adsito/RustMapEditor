using UnityEngine;

[ExecuteAlways]
public class MonumentInfo : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}