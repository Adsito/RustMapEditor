using UnityEngine;

[ExecuteAlways]
public class Monument : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}