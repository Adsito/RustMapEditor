using UnityEngine;

[ExecuteAlways]
public class PowerlineNode : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
