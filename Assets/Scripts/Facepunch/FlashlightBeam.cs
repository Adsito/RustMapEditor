using UnityEngine;

[ExecuteAlways]
public class FlashlightBeam : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
