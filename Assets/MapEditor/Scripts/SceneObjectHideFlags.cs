using UnityEngine;

[ExecuteInEditMode]
public class SceneObjectHideFlags : MonoBehaviour
{
    public HideFlags Flags;

    private void Start()
    {
        gameObject.hideFlags = HideFlags.None;
    }
}