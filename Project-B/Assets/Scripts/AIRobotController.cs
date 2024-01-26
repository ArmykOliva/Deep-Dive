using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRobotController : MonoBehaviour
{
	public enum State
	{
		WatchPlayer,
		GotoPlayer,
		MoveAroundIdle
	}

	public List<AudioClip> namedVoiceLines;
	private Dictionary<string, AudioClip> voiceLineDict = new Dictionary<string, AudioClip>();

	public Transform playerTransform;
	public float rotationSpeed = 5f;
	public float movementSpeed = 2f;
	public Material lightMaterial;
	public float idleMovementFrequency = 2f;

	public float maxPlayerDistance = 10f;
	public float minPlayerDistance = 5f;

	private float lastMoveTime;
	private Rigidbody rb;

	private Color originalEmissionColor;
	private float intensity;

	private float[] audioSampleData;
	private const int sampleDataLength = 1024; // Length of the audio sample array

	// Intensity of the audio, ranges from 0 to 1
	public float audioIntensity { get; private set; }

	private State currentState;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		foreach (var namedClip in namedVoiceLines)
		{
			voiceLineDict[namedClip.name] = namedClip;
		}

		currentState = State.WatchPlayer; // Default statedss

		originalEmissionColor = lightMaterial.GetColor("_EmissionColor");
		audioSampleData = new float[sampleDataLength];
	}

	private void FixedUpdate()
	{
		// Check the distance to the player
		float distanceToPlayer = Vector3.Distance(playerTransform.position, rb.position);

		// Decide the state based on the player distance
		if (distanceToPlayer > maxPlayerDistance)
		{
			currentState = State.GotoPlayer;
		}
		else if (distanceToPlayer < minPlayerDistance)
		{
			currentState = State.WatchPlayer;
		}

		switch (currentState)
		{
			case State.WatchPlayer:
				LookAtPlayer();
				break;
			case State.GotoPlayer:
				MoveToPlayer();

				break;
			case State.MoveAroundIdle:
				IdleMovement();
				break;
		}
	}

	void Update()
	{

		AudioSource audioSource = GetComponent<AudioSource>();
		if (audioSource.isPlaying)
		{
			audioSource.GetOutputData(audioSampleData, 0);
			float sum = 0;

			// Calculate the RMS value
			for (int i = 0; i < audioSampleData.Length; i++)
			{
				sum += audioSampleData[i] * audioSampleData[i]; // Sum of squares
			}
			float rmsValue = Mathf.Sqrt(sum / audioSampleData.Length); // RMS = square root of average

			// Convert RMS to a 0-1 intensity value. You might need to adjust the reference intensity according to your needs.
			float referenceIntensity = 0.1f; // Adjust this value to your desired "full" intensity
			audioIntensity = Mathf.Clamp01(rmsValue / referenceIntensity);

			intensity = 1f + audioIntensity*3;

		} else
		{
			intensity = 1f;
		}

		lightMaterial.SetColor("_EmissionColor", originalEmissionColor * intensity);
	}

	void LookAtPlayer()
	{
		if (playerTransform == null) return;

		Vector3 direction = (playerTransform.position - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		Quaternion slerpedRotation = Quaternion.Slerp(rb.rotation, lookRotation, Time.deltaTime * rotationSpeed);

		rb.MoveRotation(slerpedRotation); // Use Rigidbody's MoveRotation for rotation
	}

	void MoveToPlayer()
	{
		if (playerTransform == null) return;

		LookAtPlayer(); // Orient the object towards the player

		Vector3 movementDirection = (playerTransform.position - transform.position).normalized;
		rb.MovePosition(rb.position + movementDirection * movementSpeed * Time.deltaTime); // Use Rigidbody's MovePosition for movement
	}

	void IdleMovement()
	{
		if (Time.time - lastMoveTime > idleMovementFrequency)
		{
			// Implement random look and movement
			lastMoveTime = Time.time;
		}
	}

	public void PlayVoiceLine(string name)
	{
		if (voiceLineDict.TryGetValue(name, out AudioClip clip))
		{
			GetComponent<AudioSource>().PlayOneShot(clip);
		}
	}

	// Method to change state externally
	public void ChangeState(State newState)
	{
		currentState = newState;
	}
}
