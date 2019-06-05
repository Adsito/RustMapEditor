using UnityEngine;

[ExecuteAlways]
public class Sound : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
