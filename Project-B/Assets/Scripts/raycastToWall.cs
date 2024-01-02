using UnityEngine;

public class RaycastGizmoDrawer : MonoBehaviour
{
  public Transform tip; // Assign the tip transform
  private Vector3 hitPoint; // To store the hit point
  private bool hasHit; // To check if raycast hits anything
  public float maxDistance = 30f;

  void Update()
  {
    RaycastHit hit;
    int layerMask = LayerMask.GetMask("Wall", "Enemy");

    // Raycast from tip in the direction it's pointing
    if (Physics.Raycast(tip.position, tip.forward, out hit, maxDistance,layerMask))
    {
      hitPoint = hit.point;
      hasHit = true;
    }
    else
    {
      hasHit = false;
    }
  }

  private void OnDrawGizmos()
  {
    if (tip == null) return;

    Gizmos.color = Color.red;

    // Draw the ray line
    if (hasHit)
    {
      Gizmos.DrawLine(tip.position,hitPoint); // Line to hit point
      Gizmos.DrawSphere(hitPoint, 0.2f); // Sphere at hit point
    }
    else
    {
      Gizmos.DrawLine(tip.position, tip.position + tip.forward * maxDistance); // Line if no hit
      Gizmos.DrawSphere(tip.position + tip.forward * maxDistance, 0.1f); // Sphere at hit point
    }
  }
}
