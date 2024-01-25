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
	private float lastMoveTime;

	public float talkSpeed = 1f;
	public float maxIntensity = 2f;
	public float minIntensity = 0.5f;
	public float threshold = 0.3f;
	private Color originalEmissionColor;
	private Renderer renderer;
	private bool talking = false;
	private float intensity;
	private float targetIntensity = 1f;
	private Coroutine talkingCoroutine;

	private State currentState;

	void Start()
	{
		foreach (var namedClip in namedVoiceLines)
		{
			voiceLineDict[namedClip.name] = namedClip;
		}

		currentState = State.WatchPlayer; // Default statedss

		renderer = GetComponent<Renderer>();
		intensity = maxIntensity;
		targetIntensity = intensity;
		originalEmissionColor = lightMaterial.GetColor("_EmissionColor");
	}

	void Update()
	{
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

		if (GetComponent<AudioSource>().isPlaying)
		{
			if (intensity > targetIntensity)
			{
				intensity -= talkSpeed * Time.deltaTime;
				if (intensity <= targetIntensity) targetIntensity = maxIntensity - Random.Range(0, threshold);
			} else
			{
				intensity += talkSpeed * Time.deltaTime;
				if (intensity >= targetIntensity) targetIntensity = minIntensity + Random.Range(0, threshold);
			}
			intensity = Mathf.Clamp(intensity, 0f, maxIntensity);
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
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
	}

	void MoveToPlayer()
	{
		LookAtPlayer();
		transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, movementSpeed * Time.deltaTime);
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
