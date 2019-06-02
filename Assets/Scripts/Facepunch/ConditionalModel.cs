using UnityEngine;

[ExecuteAlways]
public class ConditionalModel : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
