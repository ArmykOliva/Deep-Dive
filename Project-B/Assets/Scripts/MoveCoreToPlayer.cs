using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCoreToPlayer : MonoBehaviour
{
    public Transform startPosition;
    public Transform endPosition;

    public ProgressTracker progressTracker;

    private float speed;

    private void Awake()
    {
        this.transform.position = startPosition.position;
    }

    private void Update()
    {
        this.transform.position = Vector3.Lerp(startPosition.position, endPosition.position, progressTracker.getProgress());
    }
}
