using UnityEngine;

[ExecuteAlways]
public class WaterVisibilityTrigger : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}