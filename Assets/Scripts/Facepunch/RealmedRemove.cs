using UnityEngine;

[ExecuteAlways]
public class RealmedRemove : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}