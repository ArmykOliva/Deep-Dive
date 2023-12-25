using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class renderLine : MonoBehaviour
{
  public Transform startPoint;
  public Transform endPoint;
  private LineRenderer lineRenderer;

  void Start()
  {
    lineRenderer = GetComponent<LineRenderer>();
  }

  void Update()
  {
    if (startPoint != null && endPoint != null)
    {
      lineRenderer.SetPosition(0, startPoint.position);
      lineRenderer.SetPosition(1, endPoint.position);
    }
  }
}
