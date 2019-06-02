using UnityEngine;

[ExecuteAlways]
public class LightGroupAtTime : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}