using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shark : MonoBehaviour
{
  public Transform target; // The target the shark is moving towards.
  public Transform core; // The core that the shark needs to avoid.
  public float coreRadius = 50f; // The radius within which the shark starts avoiding the core.
  public float swimSpeed = 5f; // The swimming speed of the shark.
  public float turnSpeed = 1f; // How quickly the shark can turn.
  private Vector3 currentVelocity;

  private void Update()
  {
    Vector3 desiredDirection = (target.position - transform.position).normalized;
    Vector3 coreAvoidanceVector = Vector3.zero;

    float distanceToCore = Vector3.Distance(transform.position, core.position);
    if (distanceToCore < coreRadius)
    {
      // Calculate a direction that moves the shark away from the core tangentially
      Vector3 toCore = (transform.position - core.position).normalized;
      coreAvoidanceVector = Vector3.Cross(toCore, Vector3.up).normalized;

      // Adjust the direction based on how close the shark is to the core
      float proximity = 1f - (distanceToCore / coreRadius); // This will be 0 when at the edge and 1 when at the center
      desiredDirection = Vector3.Lerp(desiredDirection, coreAvoidanceVector, proximity).normalized;
    }

    // Calculate smoothed velocity
    currentVelocity = Vector3.Lerp(currentVelocity, desiredDirection * swimSpeed, turnSpeed * Time.deltaTime);

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
