using System;
using UnityEngine;

public class ParticleSystemLOD : MonoBehaviour
{
    private ParticleSystem particle;

	protected void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        particle.Stop();
    }
}
