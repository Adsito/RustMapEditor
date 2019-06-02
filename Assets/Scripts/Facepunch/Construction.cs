using UnityEngine;

[ExecuteAlways]
public class Construction : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
