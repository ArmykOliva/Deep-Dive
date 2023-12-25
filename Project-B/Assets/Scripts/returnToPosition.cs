using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class returnBack : MonoBehaviour
{
  public Transform originalTransform; // Set this to the original position and rotation
  private bool isReturning = true;
  public float distanceToUngrab = 2.0f; // Maximum distance to ungrab
  public float speed = 5.0f; // Adjust this speed as needed
  public float rotationSpeed = 100.0f;
  public Autohand.Grabbable grabbable;

  private Vector3 initialPosition;
  private Quaternion initialRotation;

  private int originalLayer;
  private int ignoreCollisionLayer;

  void Start()
  {
    // Store the initial position and rotation
    initialPosition = originalTransform.position;
    initialRotation = originalTransform.rotation;

    // Save the original layer and set the layer to ignore collisions
    originalLayer = gameObject.layer;
    ignoreCollisionLayer = LayerMask.NameToLayer("IgnoreCollision"); // Make sure this layer exists in your project
  }

  void Update()
  {
    // Check distance from the original position
    if (Vector3.Distance(transform.position, initialPosition) > distanceToUngrab)
    {
      Debug.Log("UNGRAB");
      if (grabbable != null) grabbable.ForceHandsRelease();
      OnRelease(); // Auto-release if the distance is exceeded
    }

    if (isReturning)
    {
      // Calculate the step size for this frame
      float positionStep = speed * Time.deltaTime;
      float rotationStep = rotationSpeed * Time.deltaTime; // Adjust rotation speed if necessary

      // Move position and rotate towards the target
      transform.position = Vector3.MoveTowards(transform.position, initialPosition, positionStep);
      transform.rotation = Quaternion.RotateTowards(transform.rotation, initialRotation, rotationStep);

      // Check if the gun has reached its original position and rotation
      if (Vector3.Distance(transform.position, initialPosition) < 0.001f &&
          Quaternion.Angle(transform.rotation, initialRotation) < 0.1f)
      {
        gameObject.layer = originalLayer; // Reset the layer
      }
    }
  }

  // Call this method when the gun is grabbed
  public void OnGrab()
  {
    isReturning = false;
    gameObject.layer = originalLayer; // Reset the layer in case it's still ignoring collisions
  }

  // Call this method when the gun is released
  public void OnRelease()
  {
    isReturning = true;
    gameObject.layer = ignoreCollisionLayer; // Change layer to ignore collisions
  }
}
