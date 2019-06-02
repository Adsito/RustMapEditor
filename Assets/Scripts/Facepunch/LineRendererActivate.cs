using UnityEngine;

[ExecuteAlways]
public class LineRendererActivate : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
