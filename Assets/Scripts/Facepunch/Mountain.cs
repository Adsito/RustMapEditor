using UnityEngine;

[ExecuteAlways]
public class Mountain : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}