using UnityEngine;

[ExecuteAlways]
public class Door : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}