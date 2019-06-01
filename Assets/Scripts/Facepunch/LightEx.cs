using UnityEngine;

[ExecuteAlways]
public class LightEx : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}