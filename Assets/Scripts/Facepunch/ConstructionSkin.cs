using UnityEngine;

[ExecuteAlways]
public class ConstructionSkin : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}