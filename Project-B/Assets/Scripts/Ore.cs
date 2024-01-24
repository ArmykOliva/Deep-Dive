using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TreeEditor.TreeEditorHelper;
using static UnityEngine.GraphicsBuffer;

public class Ore : MonoBehaviour
{
	public GunType oreType;
	public float attractionStrength = 10f;
	private Rigidbody rb;
	private OreCollect nearestCollector;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		FindNearestCollector();
		if (nearestCollector != null)
		{
			AttractToCollector();
		}
	}

	void FindNearestCollector()
	{
		// Find all collectors in the scene
		OreCollect[] collectors = FindObjectsOfType<OreCollect>();
		float closestDistance = Mathf.Infinity;

		foreach (var collector in collectors)
		{
			float distance = (collector.transform.position - transform.position).sqrMagnitude;
			if (distance < closestDistance)
			{
				closestDistance = distance;
				nearestCollector = collector;
			}
		}
	}

	void AttractToCollector()
	{
		Vector3 directionToTarget = (nearestCollector.gameObject.transform.position - transform.position).normalized;
		Vector3 force = directionToTarget * attractionStrength;

		rb.AddForce(force);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<OreCollect>())
		{
			other.GetComponent<OreCollect>().Collect(oreType, 1); // Assuming '1' is the quantity
			Destroy(gameObject);
		}
	}
}
