using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyProjectile : EnemyBase
{
	public UnityEvent OnHitSubmarine;
	public MeshRenderer projectileMeshRenderer; // Assign this in the inspector

	private bool hasHit = false; // To check if the bullet has hit something

	void OnTriggerEnter(Collider other)
	{
		if (!hasHit)
		{
			// Check if the collided object or any of its parent objects have the Submarine component
			Submarine submarine = other.GetComponentInParent<Submarine>();
			if (submarine != null)
			{
				OnHitSubmarine?.Invoke();
				submarine.TakeDamage(damage);
			}

			// Bullet hits something, so disable its MeshRenderer and stop it from moving
			if (projectileMeshRenderer != null)
			{
				projectileMeshRenderer.enabled = false;
			}
			hasHit = true; // Stop the bullet from moving

			// Start the coroutine to destroy the bullet after 1 second
			StartCoroutine(DestroyAfterDelay());
		}
	}

	private IEnumerator DestroyAfterDelay()
	{
		// Wait for 1 second
		yield return new WaitForSeconds(1f);
		// Then destroy the bullet game object
		Destroy(gameObject);
	}

}
