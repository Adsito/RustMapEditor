using UnityEngine;

[ExecuteAlways]
public class Spawnable : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}