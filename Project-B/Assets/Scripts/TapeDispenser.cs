using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapeDispenser : MonoBehaviour
{
	public GameObject prefabToSpawn; // Assign your prefab in the Inspector
	public Transform spawnTransform;
	public PlacePoint placePoint;

	private void Start()
	{
		SpawnPrefabAtTransform();
	}

	public void SpawnPrefabAtTransform()
	{
		StartCoroutine(SpawnWithDelay());
	}

	// Coroutine for delayed spawning
	private IEnumerator SpawnWithDelay()
	{
		yield return new WaitForSeconds(0.4f); // Wait for 0.4 seconds

		if (placePoint.placedObject != null) yield break;

		if (prefabToSpawn == null)
		{
			Debug.LogError("Prefab to spawn is not assigned.");
			yield break;
		}

		Instantiate(prefabToSpawn, spawnTransform.position, spawnTransform.rotation);
	}
}
