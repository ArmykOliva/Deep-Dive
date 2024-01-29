using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FakeAIAnimationHandler : MonoBehaviour
{
    public ParticleSystem ps;
	public AudioSource audioSource;
	public Image blackScreen;


	IEnumerator explodeCoroutine()
    {

		ps.Play();
		yield return new WaitForSeconds(0.5f);

		audioSource.Play();
		

		blackScreen.color = new Color(255f, 255f, 255f, 1f);

		yield return new WaitForSeconds(10f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
