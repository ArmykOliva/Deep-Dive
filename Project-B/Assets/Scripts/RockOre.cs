using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockOre : MonoBehaviour
{
	public GameObject orePrefab;
	public int numberOfOres = 10; //was 10
	public float spreadForce = 5f;

	public void SpawnOres()
	{
		for (int i = 0; i < numberOfOres; i++)
		{
			GameObject ore = Instantiate(orePrefab, transform.position, Quaternion.identity);
			Rigidbody rb = ore.GetComponent<Rigidbody>();
			Ore oreAttraction = ore.GetComponent<Ore>();

			Vector3 randomDirection = Random.insideUnitSphere.normalized;
			rb.AddForce(randomDirection * spreadForce, ForceMode.Impulse);
		}
	}
}
