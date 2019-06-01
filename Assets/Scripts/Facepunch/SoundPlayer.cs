using UnityEngine;

[ExecuteAlways]
public class SoundPlayer : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}