using UnityEngine;

[ExecuteAlways]
public class EffectRecycle : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
