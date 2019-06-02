using UnityEngine;

[ExecuteAlways]
public class OnePoleLowpassFilter : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
