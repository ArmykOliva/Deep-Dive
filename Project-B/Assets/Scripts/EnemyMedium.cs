using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyMediumState
{
	generateRandomPoint,
	goToPoint,
	aim,
	shoot,
}

public class EnemyMedium : EnemyBase
{
	[Header("EnemyMedium")]
	public float spikeScale = 1f;
	public float maxSpeed = 5f;
	public float maxSlowSpeed = 2f;
	public float maxSprintSpeed = 8f;
	public float acceleration = 8f;
	public float aimTimer = 2f;
	public float rotationSpeed = 20.0f; // Speed at which the direction changes
	public Transform beakSpawnPosition;
	public GameObject beakPrefab;
	public EnemyBullet currentBeak;

	private Transform randomPoint;
	private Transform randomPointSubmarine;
	private bool startState = true;
	private bool firstPoint = true;
	private float speed = 0f;
	private float currentSpeed = 0f;
	private float timer;
	private Vector3 direction;
	private Vector3 currentDirection;
	private EnemyMediumState currentState = EnemyMediumState.generateRandomPoint;


	void Update()
	{
		if (dead)
		{
			Debug.Log(currentBeak);
			if (currentBeak != null) Destroy(currentBeak);
			return;
		}
		switch 	(currentState)
		{
			case EnemyMediumState.generateRandomPoint:
				startState = false;
				randomPoint = enemyBorder.CreateRandomTransportPointInCircle(transform.position, 0.4f);
				currentState = EnemyMediumState.goToPoint;
				startState = true;
				break;

			case EnemyMediumState.goToPoint:	
				if (startState)
				{
					// Set the max speed for this state
					speed = firstPoint ? maxSpeed : maxSprintSpeed;
					// Calculate the direction vector towards the random point
				}
				startState = false;

				direction = (randomPoint.position - transform.position).normalized;

				float distanceToTargetOnZ = Mathf.Abs(transform.position.z - randomPoint.position.z);
				if (distanceToTargetOnZ < 0.2f)
				{
					Destroy(randomPoint.gameObject);
					currentState = EnemyMediumState.aim; // Switch to aim state when stopped
					startState = true;
					firstPoint = false;
				}

				break;

			case EnemyMediumState.aim:
				if (startState)
				{
					speed = maxSlowSpeed;

					randomPointSubmarine = enemyBorderSubmarine.CreateRandomTransportPointInCircle(transform.position, 0.0f);
					timer = aimTimer;
					
				}
				startState = false;
				timer -= Time.deltaTime;

				direction = (randomPointSubmarine.position - transform.position).normalized;

				if (IsRotationAlignedWithDirection() && timer <= 0)
				{
					speed = 0f;
					startState = true;
					currentState = EnemyMediumState.shoot;
				}
				break;

			case EnemyMediumState.shoot:
				OnAttack?.Invoke();
				if (currentBeak != null)
				{
					currentBeak.active = true;
					currentBeak = null;
				}

				// Create a new beak
				GameObject beakObject = Instantiate(beakPrefab, beakSpawnPosition.position, transform.rotation);
				beakObject.transform.SetParent(this.transform, true);  // Set the parent to this GameObject
				EnemyBullet newBeak = beakObject.GetComponent<EnemyBullet>();

				if (newBeak != null)
				{
					newBeak.active = false; // Start inactive, and activate it when needed
					currentBeak = newBeak; // Store the reference to this new beak
				}

				startState = true;
				currentState = EnemyMediumState.generateRandomPoint;
				Destroy(randomPointSubmarine.gameObject);
				break;
		}

		// Smoothly adjust speed
		if (currentSpeed > speed)
		{
			// Decelerate if current speed is greater than max speed
			currentSpeed = Mathf.Max(currentSpeed - acceleration * Time.deltaTime, speed);
		}
		else
		{
			// Accelerate if current speed is less than max speed
			currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, speed);
		}

		// Smoothly update the currentDirection towards the target direction
		if (direction != Vector3.zero)
		{
			currentDirection = Vector3.Slerp(currentDirection, direction.normalized, rotationSpeed * Time.deltaTime);
		}

		// Normalize the currentDirection to ensure it's a unit vector
		currentDirection.Normalize();

		// Calculate new position based on the currentDirection and currentSpeed
		Vector3 newPosition = transform.position + currentDirection * currentSpeed * Time.deltaTime;
		transform.position = newPosition;

		if (currentDirection != Vector3.zero)
		{
			transform.forward = currentDirection;
		}
	}

	private bool IsRotationAlignedWithDirection()
	{
		if (direction != Vector3.zero)
		{
			// Get the forward vector of the transform and normalize it
			Vector3 forward = transform.forward;
			forward.Normalize();

			// Check the angle between forward and direction
			float angle = Vector3.Angle(forward, direction);

			// Define a threshold for how closely aligned they need to be
			float alignmentThreshold = 5.0f; // Degrees, adjust as needed

			return angle <= alignmentThreshold;
		}

		return false;
	}
}
