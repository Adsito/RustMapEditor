using UnityEngine;

[ExecuteAlways]
public class BaseOven : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
