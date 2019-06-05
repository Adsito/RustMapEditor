using UnityEngine;

[ExecuteAlways]
public class Deployable : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
