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
			voiceLineDict[namedClip.name] = namedClip;
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
			StartTalking();
			//StartCoroutine(StopTalkingAfterDelay(clip.length));
		}
	}
	IEnumerator StopTalkingAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		StopTalking();
	}

	// Method to change state externally
	public void ChangeState(State newState)
	{
		currentState = newState;
	}

	public void StartTalking()
	{
		if (talkingCoroutine != null) return;
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
			// decrease intensity
			float targetIntensity = minIntensity + Random.Range(0, threshold);
			float intensity = maxIntensity;
			while (intensity > targetIntensity)
			{
				intensity -= talkSpeed * Time.deltaTime;
				renderer.material.SetColor("_EmissionColor", originalEmissionColor * intensity);
				Debug.Log(intensity);
				yield return null;
			}

			// increase intensity
			while (intensity < maxIntensity)
			{
				intensity += talkSpeed * Time.deltaTime;
				renderer.material.SetColor("_EmissionColor", originalEmissionColor * intensity);
				yield return null;
			}
		}
	}

	void ResetEmissionColor()
	{
		renderer.material.SetColor("_EmissionColor", originalEmissionColor * 1f	);
	}
}
