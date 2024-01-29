using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class bullet : MonoBehaviour
{
	public float speed = 5f;
	public int damage = 1;
	public float maxDistance = 300f;

	public UnityEvent OnHitWall;

	private Vector3 startPosition;
	private bool hasHit = false; // To check if the bullet has hit something

	private void Start()
	{
		startPosition = transform.position;
	}

	void Update()
	{
		// Only move the bullet forward if it hasn't hit anything
		if (!hasHit)
		{
			transform.position += transform.forward * speed * Time.deltaTime;

			// Check the distance traveled
			if (Vector3.Distance(startPosition, transform.position) > maxDistance)
			{
				// Destroy the bullet if it exceeds the maximum distance
				Destroy(gameObject);
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (!hasHit)
		{
			IDamageable damageable = other.GetComponent<IDamageable>();
			if (damageable == null) damageable = other.GetComponentInParent<IDamageable>();
			if (damageable != null)
			{
				damageable.TakeDamage(damage);
			}
			else
			{
				OnHitWall?.Invoke();
			}

			// Bullet hits something, so disable its MeshRenderer and stop it from moving
			DisableAllMeshRenderers();
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

	private void DisableAllMeshRenderers()
	{
		// Disable the MeshRenderer of the current GameObject
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		if (meshRenderer != null)
		{
			meshRenderer.enabled = false;
		}

		// Disable all MeshRenderers in child GameObjects
		foreach (Transform child in transform)
		{
			meshRenderer = child.GetComponent<MeshRenderer>();
			if (meshRenderer != null)
			{
				meshRenderer.enabled = false;
			}
		}
	}
}