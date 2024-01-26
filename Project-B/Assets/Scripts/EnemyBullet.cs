using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBullet : MonoBehaviour, IDamageable
{
  public float speed = 5f;
	public int hp = 2;
  public int damage = 1;
  public float maxDistance = 100f;
  public bool active = false;

	public UnityEvent OnHitSubmarine;
	public MeshRenderer bulletMeshRenderer; // Assign this in the inspector

	private Collider bulletCollider;
	private Vector3 startPosition;
	private Vector3 originalScale;
	private float scaleUpTime = 1.0f; // Duration to scale up
	private float scaleTimer = 0f; // Timer for scaling up
	private bool hasHit = false; // To check if the bullet has hit something


	private void Start()
	{
    startPosition = transform.position;
		originalScale = new Vector3(1f,1f,1f); // Save the original scale
		transform.localScale = Vector3.zero; // Set scale to zero
		bulletCollider = GetComponent<Collider>();
	}

	void Update()
  {
		if (!hasHit)
		{
			// Scale up the bullet over time
			if (scaleTimer < scaleUpTime)
			{
				scaleTimer += Time.deltaTime;
				float progress = scaleTimer / scaleUpTime;
				transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
			}

			if (active)
			{
				// Detach from parent when first activated
				if (transform.parent != null)
				{
					transform.parent = null; // This makes the bullet an independent GameObject
				}

				// Move the bullet forward each frame
				transform.position += transform.forward * speed * Time.deltaTime;

				// Check the distance traveled
				if (Vector3.Distance(startPosition, transform.position) > maxDistance)
				{
					// Destroy the bullet if it exceeds the maximum distance
					Destroy(gameObject);
				}
			}
			else
			{
				startPosition = transform.position;
			}
		}

		//enable and siable collider
		if (!active)
		{
			if (bulletCollider != null) bulletCollider.enabled = true;
		} else
		{
			if (bulletCollider != null) bulletCollider.enabled = false;
		}
  }

  void OnTriggerEnter(Collider other)
  {
		if (!hasHit)
		{
			if (active)
			{
				// Check if the collided object or any of its parent objects have the Submarine component
				Submarine submarine = other.GetComponentInParent<Submarine>();
				if (submarine != null)
				{
					OnHitSubmarine?.Invoke();
					submarine.TakeDamage(damage);

					// Bullet hits something, so disable its MeshRenderer and stop it from moving
					if (bulletMeshRenderer != null)
					{
						bulletMeshRenderer.enabled = false;
					}
					hasHit = true; // Stop the bullet from moving

					// Start the coroutine to destroy the bullet after 1 second
					StartCoroutine(DestroyAfterDelay());
				}
				else
				{
					if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
					{
						// Bullet hits something, so disable its MeshRenderer and stop it from moving
						if (bulletMeshRenderer != null)
						{
							bulletMeshRenderer.enabled = false;
						}
						hasHit = true; // Stop the bullet from moving

						// Start the coroutine to destroy the bullet after 1 second
						StartCoroutine(DestroyAfterDelay());
					}
				}
			}
		}
  }

	private IEnumerator DestroyAfterDelay()
	{
		// Wait for 1 second
		yield return new WaitForSeconds(1f);
		// Then destroy the bullet game object
		Destroy(gameObject);
	}

	public void TakeDamage(int damage)
	{
    if (active)
    {
			hp--;
			if (hp < 0) Destroy(gameObject);
		} 
	}
}
