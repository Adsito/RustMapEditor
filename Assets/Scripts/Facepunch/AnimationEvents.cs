using UnityEngine;

[ExecuteAlways]
public class AnimationEvents : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
