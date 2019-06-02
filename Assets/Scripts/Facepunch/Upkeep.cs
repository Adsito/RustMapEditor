using UnityEngine;

[ExecuteAlways]
public class Upkeep : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
