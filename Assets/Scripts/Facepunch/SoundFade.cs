using UnityEngine;

[ExecuteAlways]
public class SoundFade : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}
