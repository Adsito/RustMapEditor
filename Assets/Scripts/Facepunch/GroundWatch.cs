using UnityEngine;

[ExecuteAlways]
public class GroundWatch : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
