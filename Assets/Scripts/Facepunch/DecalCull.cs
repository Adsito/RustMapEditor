using UnityEngine;

[ExecuteAlways]
public class DecalCull : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}