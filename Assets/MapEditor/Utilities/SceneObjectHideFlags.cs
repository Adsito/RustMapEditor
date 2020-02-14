using UnityEngine;

[ExecuteAlways]
public class SceneObjectHideFlags : MonoBehaviour
{
    public HideFlags Flags;

    void Start()
    {
        gameObject.hideFlags = Flags;
    }
}