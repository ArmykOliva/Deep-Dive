using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
  public float speed = 5f;
  public int damage = 1;
  public float maxDistance = 300f;

  private Vector3 startPosition;

  private void Start()
	{
    startPosition = transform.position;
	}

	void Update()
  {
    // Move the bullet forward each frame
    transform.position += transform.forward * speed * Time.deltaTime;


    // Check the distance traveled
    if (Vector3.Distance(startPosition, transform.position) > maxDistance)
    {
      // Destroy the bullet if it exceeds the maximum distance
      Destroy(gameObject);
    }
  }

  void OnTriggerEnter(Collider other)
  {
    /*Enemy enemy = other.GetComponent<Enemy>();
    if (enemy != null)
    {
        enemy.TakeDamage(damageAmount);
    }*/

    // Destroy the bullet after it triggers something
    Destroy(gameObject);
  }
}
