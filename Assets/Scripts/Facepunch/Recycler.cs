using UnityEngine;

[ExecuteAlways]
public class Recycler : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
