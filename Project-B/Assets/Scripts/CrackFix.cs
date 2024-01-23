using System.Collections;
using Autohand;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class CrackFix : MonoBehaviour
{
	public ParticleSystem particleSystemToStop;
	public float fadeOutDuration = 2.0f; // Duration in seconds for the fade-out effect
	public PlacePoint placePoint = new PlacePoint();
	public int damageHp = 10;

	private Submarine submarine;
	private GameObject tapeThatFixed;
	private bool fixd = false;

	private void Start()
	{
		submarine = GetComponentInParent<Submarine>();
	}

	// Call this function to start the fade-out process
	public void Fix()
	{
		if (fixd) return;
		tapeThatFixed = placePoint.placedObject.gameObject;
		fixd = true;
		if (particleSystemToStop != null)
		{
			particleSystemToStop.Stop(); // Stop the particle system
		}

		StartCoroutine(FadeOutAndDeactivate());
	}

	private IEnumerator FadeOutAndDeactivate()
	{
		yield return new WaitForSeconds(fadeOutDuration);

		if (submarine != null)
		{
			submarine.Fix(this);
		}

		gameObject.SetActive(false); // Deactivate the GameObject
		if (tapeThatFixed != null) Destroy(tapeThatFixed);
		fixd = false;
	}
}
