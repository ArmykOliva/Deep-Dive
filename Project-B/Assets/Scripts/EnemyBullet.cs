using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBullet : MonoBehaviour, IDamageable
{
  public float speed = 5f;
  public int damage = 1;
  public float maxDistance = 100f;
  public bool active = false;

	public UnityEvent OnHitSubmarine;

	private Vector3 startPosition;
	private Vector3 originalScale;
	private float scaleUpTime = 1.0f; // Duration to scale up
	private float scaleTimer = 0f; // Timer for scaling up


	private void Start()
	{
    startPosition = transform.position;
		originalScale = new Vector3(1f,1f,1f); // Save the original scale
		transform.localScale = Vector3.zero; // Set scale to zero
	}

	void Update()
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
    } else
    {
			startPosition = transform.position;
		}
  }

  void OnTriggerEnter(Collider other)
  {
    if (active)
    {
			// Check if the collided object or any of its parent objects have the Submarine component
			Submarine submarine = other.GetComponentInParent<Submarine>();
			if (submarine != null)
			{
				OnHitSubmarine?.Invoke();
				submarine.hp--; // Decrease the hp of the Submarine
			}

			// Destroy the bullet after it triggers something
			Destroy(gameObject);
		}
  }

	public void TakeDamage(int damage)
	{
		Destroy(gameObject);
	}
}
