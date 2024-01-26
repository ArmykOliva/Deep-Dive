using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlipToCore : MonoBehaviour
{
  public GameObject cave;
  public GameObject core;
  public GameObject shark;
  public Image blackScreen;
	public float fadeDuration;

  // Start is called before the first frame update
  public void FlipCore()
  {
		StartCoroutine(FlipCoreRoutine());
  }

		

	private IEnumerator FlipCoreRoutine()
	{

		// Fade to black
		float elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			float alpha = elapsedTime / fadeDuration;
			blackScreen.color = new Color(blackScreen.color.r, blackScreen.color.g, blackScreen.color.b, alpha);
			yield return null;
		}

		yield return new WaitForSeconds(1f);


		RenderSettings.fogStartDistance = 100f;
		RenderSettings.fogEndDistance = 200f;

		cave.SetActive(false);
		core.SetActive(true);
		shark.SetActive(true);

		yield return new WaitForSeconds(3f);

		// Fade out
		elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			float alpha = 1 - (elapsedTime / fadeDuration);
			blackScreen.color = new Color(0, 0, 0, alpha);
			yield return null;
		}
	}
}
