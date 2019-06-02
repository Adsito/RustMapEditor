using UnityEngine;

[ExecuteAlways]
public class Lift : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
