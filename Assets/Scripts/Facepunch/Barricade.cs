using UnityEngine;

[ExecuteAlways]
public class Barricade : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
