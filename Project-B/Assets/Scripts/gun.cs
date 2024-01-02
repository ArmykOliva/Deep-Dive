using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gun : MonoBehaviour
{
  [Header("Gun objects and tips")]
  public Transform origin;

  public Transform minigunTip;
  public Transform shotugnTip;
  public Transform laserTip;
  public Transform gunAim;

  [Header("Shooting variables")]
  public float aimDistance = 15f; //distance where to aim

  public GameObject minigunBulletPrefab;

  public float angleLimitShooting = 70f;

  [Header("Animation")]
  public float angleLimitAnimation = 2f;
  public float rotationDampingAnimation = 0.2f;

  private GameObject currentBulletPrefab;
  private Transform currentTip;

  void Start()
  {
    currentBulletPrefab = minigunBulletPrefab;
    currentTip = minigunTip;
  }

  void Update()
  {
    RaycastHit hit;
    int layerMask = LayerMask.GetMask("Wall", "Enemy");
    Vector3 targetPoint;

    // Ray from the current position forward
    Ray ray = new Ray(gunAim.position, gunAim.forward);

    // Perform the Raycast
    if (Physics.Raycast(ray, out hit, aimDistance, layerMask))
    {
      // Ray hit an object, use the hit point
      targetPoint = hit.point;
    }
    else
    {
      // Ray didn't hit, use a point 20 meters away
      targetPoint = ray.origin + ray.direction * aimDistance;
    }

    //look at the point
    LookAtWithLimits(currentTip, targetPoint, angleLimitShooting, 0);
    //animation
    LookAtWithLimits(origin, targetPoint, angleLimitAnimation, rotationDampingAnimation);
  }

  void LookAtWithLimits(Transform objectToRotate, Vector3 targetPoint, float angleLimit, float rotationDamping)
  {
    // For this example i'll be using the root objects forward vector as what i want to be my reference constraint vector.
    // You might want to use something else though.
    Vector3 defaultFacing = transform.forward;

    // The direction from this transform, pointing at the look at target.
    Vector3 directionToLookAtTarget = targetPoint - objectToRotate.position;


    float angle = Vector3.Angle(directionToLookAtTarget, defaultFacing);

    // Since i'm just using the root objects forward vector as a constraint, i can just use its rotation as my default rotation instead of calculation a Quaternion.LookAt.
    Quaternion defaultRotation = transform.rotation;
    // The look at rotation to the target if it were completely unrestrained.
    Quaternion lookAtCompleteRotation = Quaternion.LookRotation(directionToLookAtTarget);

    Quaternion finalRotation = Quaternion.identity;

    // If the angle is greater than our limit, return a rotation that is in the direction of the lookAtCompleteRotation but is limited to the angle we chose as a limit.
    // Otherwise, if its within our limit, we just return the rotation as is.
    if (angle > angleLimit)
      finalRotation = Quaternion.Slerp(defaultRotation, lookAtCompleteRotation, angleLimit / angle);
    else
      finalRotation = lookAtCompleteRotation;

    //smooth or not smooth rotate
    if (rotationDamping != 0)
      objectToRotate.rotation = Quaternion.Slerp(objectToRotate.rotation, finalRotation, Time.deltaTime * rotationDamping);
    else
      objectToRotate.rotation = finalRotation;
  }

  public void Shoot()
	{
    GameObject bullet = Instantiate(currentBulletPrefab, currentTip.position, currentTip.rotation);
  }
}


