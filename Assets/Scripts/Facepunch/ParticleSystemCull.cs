using UnityEngine;

public class ParticleSystemCull : MonoBehaviour
{
    private ParticleSystem particle;

    protected void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        particle.Stop();
    }
}
