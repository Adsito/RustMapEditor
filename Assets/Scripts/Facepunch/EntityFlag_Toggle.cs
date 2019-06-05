using UnityEngine;

[ExecuteAlways]
public class EntityFlag_Toggle : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
