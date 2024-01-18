using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveMover : MonoBehaviour
{
	public float speed = 5f; // Speed of the object

	void Update()
	{
		// Move the object forward along the Z-axis at the specified speed
		transform.Translate(Vector3.forward * -speed * Time.deltaTime, Space.World);
	}
}
