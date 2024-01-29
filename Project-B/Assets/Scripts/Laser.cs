using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
  public int damage = 20;
  [HideInInspector] public float strength = 1.0f;

  public float maxDistance = 300f;
  public LineRenderer lineRenderer; // Assign this in the Inspector

  private Vector3 startPosition;

  private void Start()
  {
    lineRenderer.startWidth *= strength;
    lineRenderer.endWidth *= strength;

    startPosition = transform.position;

    // Perform a raycast
    if (Physics.Raycast(startPosition, transform.forward, out RaycastHit hit, maxDistance))
    {
      SetLine(startPosition, hit.point);

			IDamageable damageable = hit.collider.GetComponent<IDamageable>();
			if (damageable == null) damageable = hit.collider.GetComponentInParent<IDamageable>();
			if (damageable != null)
			{
				damageable.TakeDamage(Mathf.RoundToInt(damage * Mathf.Clamp01(strength+0.2f)));
			}
		}
    else
    {
      SetLine(startPosition, startPosition + transform.forward * maxDistance);
    }

    StartCoroutine(FadeOutLine());
  }

  private void SetLine(Vector3 start, Vector3 end)
  {
    lineRenderer.SetPosition(0, start);
    lineRenderer.SetPosition(1, end);
  }

  private IEnumerator FadeOutLine()
  {
    float duration = 0.5f;
    float elapsed = 0f;
    Color startColor = lineRenderer.material.color;

    while (elapsed < duration)
    {
      float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
      lineRenderer.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
      elapsed += Time.deltaTime;
      yield return null;
    }

    Destroy(gameObject);
  }
}
