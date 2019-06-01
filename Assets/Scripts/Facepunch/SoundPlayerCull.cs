using UnityEngine;

[ExecuteAlways]
public class SoundPlayerCull : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}