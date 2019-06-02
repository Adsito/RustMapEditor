using UnityEngine;

[ExecuteAlways]
public class SocketHandle : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
