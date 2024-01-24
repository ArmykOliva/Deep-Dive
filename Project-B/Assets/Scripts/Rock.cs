using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Rock : EnemyBase
{
	public List<GameObject> rockPrefabs; // List of rock prefabs to choose from
	public float minScale = 0.8f;        // Minimum scale for the rocks
	public float maxScale = 1.2f;        // Maximum scale for the rocks
	public float rotationSpeedMin = 10f; // Minimum rotation speed
	public float rotationSpeedMax = 50f; // Maximum rotation speed

	public UnityEvent OnHitSubmarine;

	private MeshRenderer projectileMeshRenderer; // Assign this in the inspector
	private bool hasHit = false; // To check if the bullet has hit something
	private float rotationSpeed; // Actual rotation speed
	private GameObject rockInstance;
	private Vector3 rotationAxis;        // Axis of rotation

	void Start()
	{
		if (rockPrefabs.Count == 0)
		{
			Debug.LogError("No rock prefabs assigned in the RockSpawner script!");
			return;
		}

		// Choose a random prefab from the list
		GameObject selectedPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];

		// Instantiate the prefab
		rockInstance = Instantiate(selectedPrefab, transform.position, Quaternion.identity);

		projectileMeshRenderer = rockInstance.GetComponent<MeshRenderer>();

		// Set the rock instance as a child of this GameObject
		rockInstance.transform.SetParent(transform, false);
		rockInstance.transform.localPosition = Vector3.zero;
		rockInstance.layer = gameObject.layer;

		// Choose a random scale from minScale to maxScale
		float scale = Random.Range(minScale, maxScale);

		// Apply the random scale to the rock instance
		rockInstance.transform.localScale = new Vector3(scale, scale, scale);

		// Set a random rotation speed
		rotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax);

		// Choose a random rotation axis
		rotationAxis = Random.onUnitSphere; // This gives a random direction vector

		//for flashing
		rend = rockInstance.GetComponent<Renderer>();
		if (rend != null)
		{
			normalMaterial = rend.material; // Set normalMaterial to the current material
		}
	}

	void Update()
	{
		// Rotate the rock around the chosen axis at the chosen speed
		if (rockInstance != null)
		{
			rockInstance.transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
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
			

				// Bullet hits something, so disable its MeshRenderer and stop it from moving
				if (projectileMeshRenderer != null)
				{
					projectileMeshRenderer.enabled = false;
				}
				hasHit = true; // Stop the bullet from moving

				// Start the coroutine to destroy the bullet after 1 second
				StartCoroutine(DestroyAfterDelay());
			}
			else
			{
				// Check if the collided object's layer is 'Wall'
				if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
				{
					// Destroy the bullet immediately
					Destroy(gameObject);
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

}
