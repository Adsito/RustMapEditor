using UnityEngine;

[ExecuteAlways]
public class LightOccludee : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
