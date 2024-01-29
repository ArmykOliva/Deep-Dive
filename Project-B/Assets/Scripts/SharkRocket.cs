using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SharkRocket : MonoBehaviour, IDamageable
{
	public int hp = 2;
	public int damage = 1;

	public UnityEvent OnHitSubmarine;
	public MeshRenderer bulletMeshRenderer; // Assign this in the inspector

	private bool hasHit = false; // To check if the bullet has hit something

	public Transform target; // The target the shark is moving towards.
  public Transform core; // The core that the shark needs to avoid.
  public float coreRadius = 50f; // The radius within which the shark starts avoiding the core.
  public float swimSpeed = 5f; // The swimming speed of the shark.s
  public float turnSpeed = 1f; // How quickly the shark can turn.
  private Vector3 currentVelocity;

	private int state = 0;

	private void Start()
	{
		StartCoroutine(BeginMovingForward());
	}

	private IEnumerator BeginMovingForward()
	{
		state = 0;
		yield return new WaitForSeconds(0.5f);
		state = 1;
	}


	private void Update()
	{
		if (hasHit)
		{
			if (bulletMeshRenderer != null)
			{
				bulletMeshRenderer.enabled = false;
			}
			return;
		}

        if (state == 0)
        {
			transform.position += transform.forward * swimSpeed*2f * Time.deltaTime;
		} else
		{
			Vector3 toTarget = (target.position - transform.position).normalized;
			Vector3 toCore = (core.position - transform.position).normalized;
			Vector3 toCoreFromTarget = (core.position - target.position).normalized;

			// Calculate the normal of the plane defined by the shark, the core, and the target
			Vector3 planeNormal = Vector3.Cross(toCore, toCoreFromTarget).normalized;

			// Determine whether the target is to the left or right side of the shark
			float directionSign = Mathf.Sign(Vector3.Dot(planeNormal, Vector3.Cross(toCore, toTarget)));

			// Calculate the direction to avoid the core
			Vector3 coreAvoidanceDirection = Vector3.Cross(planeNormal, toCore).normalized * directionSign;

			// Check if we need to avoid the core
			float distanceToCore = Vector3.Distance(transform.position, core.position);
			if (distanceToCore < coreRadius)
			{
				float proximity = (coreRadius - distanceToCore) / coreRadius;
				toTarget = Vector3.Lerp(toTarget, coreAvoidanceDirection, proximity).normalized;
			}

			// Calculate smoothed velocity
			currentVelocity = Vector3.Lerp(currentVelocity, toTarget * swimSpeed, turnSpeed * Time.deltaTime);

			// Move the shark forward along the calculated velocity
			transform.position += currentVelocity * Time.deltaTime;

			// Rotate the shark to face the current velocity
			if (currentVelocity != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
			}
		}
	}


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
				hasHit = true; // Stop the bullet from moving

				// Start the coroutine to destroy the bullet after 1 second
				StartCoroutine(DestroyAfterDelay());
			}
			else
			{
				if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
				{

					hasHit = true; // Stop the bullet from moving

					// Start the coroutine to destroy the bullet after 1 second
					StartCoroutine(DestroyAfterDelay());
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
		hp--;
		if (hp <= 0) Destroy(gameObject);
	}
}
