using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerParticles : MonoBehaviour
{
	private ParticleSystem particleSystem;

	private void Awake()
	{
		particleSystem = GetComponent<ParticleSystem>();
	}

	public void PlayParticles()
	{
		if (particleSystem != null)
		{
			// Play the particle system
			particleSystem.Play();

			// Optional: Destroy the particle system GameObject after the particles have finished
			Destroy(gameObject, particleSystem.main.duration);
		}
	}
}
