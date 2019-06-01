using UnityEngine;

[ExecuteAlways]
public class Model : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}