using UnityEngine;

[ExecuteAlways]
public class CommentComponent : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
