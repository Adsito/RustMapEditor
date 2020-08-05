using UnityEngine;

[ExecuteInEditMode]
public class SceneObjectHideFlags : MonoBehaviour
{
    public HideFlags Flags;

    private void Start()
    {
        gameObject.hideFlags = Flags;
    }

    public void ToggleHideFlags(bool enabled) 
    {
        gameObject.hideFlags = enabled ? Flags : HideFlags.None;
    }
}