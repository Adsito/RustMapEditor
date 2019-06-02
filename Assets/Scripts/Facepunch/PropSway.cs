using UnityEngine;

[ExecuteAlways]
public class PropSway : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
