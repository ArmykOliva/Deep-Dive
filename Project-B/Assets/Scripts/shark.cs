using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class shark : MonoBehaviour
{
    public Transform target; // The target the shark is moving towards.
    public Transform core; // The core that the shark needs to avoid.
    public Transform rocketTarget;
    public float coreRadius = 50f; // The radius within which the shark starts avoiding the core.
    public float swimSpeed = 5f; // The swimming speed of the shark.
    public float turnSpeed = 1f; // How quickly the shark can turn.
    private Vector3 currentVelocity;

    public GameObject rocket;
    public Transform spawnPoint;

    public List<Transform> destinationPoints;

    [SerializeField] private int currentIndex = 0;

    private bool pointFixed;

    private float timer = 0;
    public float attackTimeInterval = 10f;

    private void Start()
    {
        if(destinationPoints.Count > 0)
        {
            target = destinationPoints[currentIndex];
            Debug.Log("Going to " + currentIndex);
        }
    }

    private void Update()
    {
        
        Debug.Log("Target: " + currentIndex % destinationPoints.Count());

        timer += Time.deltaTime;
        
        Vector3 toTarget = (target.position - transform.position).normalized;
        Vector3 toCore = (core.position - transform.position).normalized;
        Vector3 toCoreFromTarget = (core.position - target.position).normalized;

        // Calculate the normal of the plane defined by the shark, the core, and the target
        Vector3 planeNormal = Vector3.Cross(toCore, toCoreFromTarget).normalized;

        // Determine whether the target is to the left or right side of the shark
        float directionSign = Mathf.Sign(Vector3.Dot(planeNormal, Vector3.Cross(toCore, toTarget)));

        // Calculate the direction to avoid the core
        Vector3 coreAvoidanceDirection = Vector3.Cross(planeNormal, toCore).normalized * directionSign;

        // Check if we need to avoid the core
        float distanceToCore = Vector3.Distance(transform.position, core.position);
        if (distanceToCore < coreRadius)
        {
            float proximity = (coreRadius - distanceToCore) / coreRadius;
            toTarget = Vector3.Lerp(toTarget, coreAvoidanceDirection, proximity).normalized;
        }

        // Calculate smoothed velocity
        currentVelocity = Vector3.Lerp(currentVelocity, toTarget * swimSpeed, turnSpeed * Time.deltaTime);

        // Move the shark forward along the calculated velocity
        transform.position += currentVelocity * Time.deltaTime;

        // Rotate the shark to face the current velocity
        if (currentVelocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        
        // Go Next
        if (Vector3.Distance(transform.position, target.position) < 5f)
        {
            Debug.Log("Arrived");
            goToNext();
        }
        
        //Spawn Rockets
        if (timer >= attackTimeInterval)
        {
            Debug.Log("Attack!!!");
            timer = 0;
            StartCoroutine(spawnRockets());
            this.GetComponent<Animator>().SetTrigger("OpenMouth");
            
        }
        
    }

    IEnumerator spawnRockets()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject rocket = Instantiate(this.rocket, spawnPoint.position, Quaternion.identity);
            rocket.GetComponent<SharkRocket>().core = this.core;
            rocket.GetComponent<SharkRocket>().target= this.rocketTarget;
            yield return new WaitForSeconds(1f);
        }
    }
    
   

    private void goToNext()
    {

        Debug.Log("Arrived  at: " + currentIndex % destinationPoints.Count + " | Going to " + (currentIndex+1) % destinationPoints.Count);
        target = destinationPoints[currentIndex++ % destinationPoints.Count];


    }
}