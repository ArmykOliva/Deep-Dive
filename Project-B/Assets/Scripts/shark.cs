using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class shark : EnemyBase
{
    public Transform target; // The target the shark is moving towards.
    public Transform core; // The core that the shark needs to avoid.
    public Transform rocketTarget;
    public float coreRadius = 50f; // The radius within which the shark starts avoiding the core.
    public float swimSpeed = 5f; // The swimming speed of the shark.
    public float turnSpeed = 1f; // How quickly the shark can turn.
    public float rocketNumber = 3;

    public GameObject rocket;
    public Transform spawnPoint;

    public List<Transform> destinationPoints;

    [SerializeField] private int currentIndex = 0;

    private bool pointFixed;

    private float timer = 0;
    public float attackTimeInterval = 10f;

    private Quaternion targetRotation;
    private Vector3 currentVelocity;
  private bool stopShooting = false;

    private void Start()
    {
        if (destinationPoints.Count > 0)
        {
            target = destinationPoints[currentIndex];
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (target != null)
        {
            // Determine the direction to the target
            Vector3 targetDirection = (target.position - transform.position).normalized;

            // Only continue if the target is not too close
            if (targetDirection != Vector3.zero)
            {
                // Determine the target rotation towards the target direction
                targetRotation = Quaternion.LookRotation(targetDirection);

                // Slerp the rotation towards the target rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            // Move the enemy forward in the direction it's facing
            transform.position += transform.forward * swimSpeed * Time.deltaTime;

            // Check if it's time to move to the next target point
            if (Vector3.Distance(transform.position, target.position) < 10f)
            {
                goToNext();
            }
        }

        // Spawn Rockets
        if (timer >= attackTimeInterval)
        {
            timer = 0;
            if (!stopShooting) StartCoroutine(spawnRockets());
            GetComponent<Animator>().SetTrigger("OpenMouth");
        }
    }

    IEnumerator spawnRockets()
    {
        for (int i = 0; i < rocketNumber; i++)
        {
            GameObject rocket =
                Instantiate(this.rocket, spawnPoint.position, transform.rotation); // Use the shark's current rotation
            SharkRocket sharkRocketComponent = rocket.GetComponent<SharkRocket>();

            if (sharkRocketComponent != null)
            {
                sharkRocketComponent.core = this.core;
                sharkRocketComponent.target = this.rocketTarget;
            }
            else
            {
                Debug.LogError("SharkRocket component not found on the instantiated rocket object.");
            }

            yield return new WaitForSeconds(1f);
        }
    }

  public void stopShootingFunction()
  {
    stopShooting = true;
  }


    private void goToNext()
    {
        target = destinationPoints[currentIndex++ % destinationPoints.Count];
    }
}