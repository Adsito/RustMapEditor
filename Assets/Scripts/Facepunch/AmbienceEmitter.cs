using UnityEngine;

[ExecuteAlways]
public class AmbienceEmitter : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
