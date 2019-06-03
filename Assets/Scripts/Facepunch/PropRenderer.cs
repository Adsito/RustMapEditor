using UnityEngine;

[ExecuteAlways]
public class PropRenderer : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
