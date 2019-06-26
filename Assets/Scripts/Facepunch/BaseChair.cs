using UnityEngine;

[ExecuteAlways]
public class BaseChair : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
