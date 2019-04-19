using UnityEngine;

public class ParticleSystemCull : MonoBehaviour
{
    private ParticleSystem particleSystem;

    protected void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystem.Stop();
    }
}
