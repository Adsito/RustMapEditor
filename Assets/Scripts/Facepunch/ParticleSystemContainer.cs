using UnityEngine;

[ExecuteAlways]
public class ParticleSystemContainer : MonoBehaviour
{
    protected void Awake()
    {
        DestroyImmediate(this);
    }
}