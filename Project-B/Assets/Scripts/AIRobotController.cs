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

	[System.Serializable]
	public struct NamedAudioClip
	{
		public string name;
		public AudioClip clip;
	}

	public NamedAudioClip[] namedVoiceLines;
	private Dictionary<string, AudioClip> voiceLineDict = new Dictionary<string, AudioClip>();

	public Transform playerTransform;
	public float rotationSpeed = 5f;
	public float movementSpeed = 2f;
	public float idleMovementFrequency = 2f;
	private float lastMoveTime;

	public float talkSpeed = 1f;
	public float maxIntensity = 2f;
	public float minIntensity = 0.5f;
	public float threshold = 0.3f;
	private Color originalEmissionColor;
	private Renderer renderer;
	private Coroutine talkingCoroutine;

	private State currentState;

	void Start()
	{
		foreach (var namedClip in namedVoiceLines)
		{
			voiceLineDict[namedClip.name] = namedClip.clip;
		}

		currentState = State.WatchPlayer; // Default statedss

		renderer = GetComponent<Renderer>();
		originalEmissionColor = renderer.material.GetColor("_EmissionColor");
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
	}

	void LookAtPlayer()
	{
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

	public void StartTalking()
	{
		if (talkingCoroutine != null)
		{
			StopCoroutine(talkingCoroutine);
		}
		talkingCoroutine = StartCoroutine(Talking());
	}

	public void StopTalking()
	{
		if (talkingCoroutine != null)
		{
			StopCoroutine(talkingCoroutine);
			talkingCoroutine = null;
		}
		ResetEmissionColor();
	}

	IEnumerator Talking()
	{
		while (true) // Replace with a condition to stop talking if needed
		{
			// Increase intensity
			float targetIntensity = maxIntensity + Random.Range(0, threshold);
			while (GetEmissionIntensity() < targetIntensity)
			{
				IncreaseIntensity(talkSpeed);
				yield return null;
			}

			// Decrease intensity
			targetIntensity = minIntensity - Random.Range(0, threshold);
			while (GetEmissionIntensity() > targetIntensity)
			{
				DecreaseIntensity(talkSpeed);
				yield return null;
			}
		}
	}

	void IncreaseIntensity(float speed)
	{
		float currentIntensity = GetEmissionIntensity();
		renderer.material.SetColor("_EmissionColor", originalEmissionColor * Mathf.Clamp(currentIntensity + speed * Time.deltaTime, minIntensity, maxIntensity));
	}

	void DecreaseIntensity(float speed)
	{
		float currentIntensity = GetEmissionIntensity();
		renderer.material.SetColor("_EmissionColor", originalEmissionColor * Mathf.Clamp(currentIntensity - speed * Time.deltaTime, minIntensity, maxIntensity));
	}

	float GetEmissionIntensity()
	{
		return renderer.material.GetColor("_EmissionColor").maxColorComponent; // Assuming the emission color is uniform
	}

	void ResetEmissionColor()
	{
		renderer.material.SetColor("_EmissionColor", originalEmissionColor);
	}
}
