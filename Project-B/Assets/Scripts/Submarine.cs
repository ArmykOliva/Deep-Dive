using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Submarine : MonoBehaviour
{
	public int maxHp = 100;
	[HideInInspector]
	public int hp;
	public Animator lightAnimator;
	public Animator panelAnimator;
	public waveSpawner waveSpawner;
	public List<CrackFix> cracks;

	[Header("death ui")]
	public Image blackScreen;
	public TextMeshProUGUI deathText;
	public float fadeDuration = 2f;

	private bool dead = false;
	private int damagedHp;
	private AudioSource audioSource;

	private void Start()
	{
		hp = maxHp;
		damagedHp = hp;
		audioSource = GetComponent<AudioSource>();
	}

	private void Update()
	{
		Debug.Log(hp);
		Debug.Log(damagedHp);
		if (damagedHp <= 0) {
			PlayerDied();
		}
	}

	public void TakeDamage(int damage)
	{
		hp -= damage;
		lightAnimator.SetTrigger("Fade");
		panelAnimator.SetTrigger("Fade");

		CreateDamage();
	}
	public void Fix(CrackFix fixedCrack)
	{
		damagedHp += fixedCrack.damageHp;
		hp += fixedCrack.damageHp;
	}

	private void CreateDamage()
	{
		///iterate randomly but with largest damageHp on top
		// Filter out cracks where crackPrefab is not active, then sort and shuffle
		var activeCracks = cracks
				.Where(crack => !crack.gameObject.activeSelf) // Filtering active crackPrefabs
				.OrderByDescending(crack => crack.damageHp)
				.ToList();

		System.Random rng = new System.Random();
		var groupedAndShuffledCracks = activeCracks
				.GroupBy(crack => crack.damageHp)
				.SelectMany(group => group.OrderBy(_ => rng.Next()));

		foreach (var crack in groupedAndShuffledCracks)
		{
			if (hp - damagedHp <= -crack.damageHp)
			{
				damagedHp -= crack.damageHp;
				crack.gameObject.SetActive(true);
			}
		}
	}

	public void PlayerDied()
	{
		if (dead) return;
		dead = true;
		audioSource.Play();
		waveSpawner.RestartWave();

		foreach (var crack in cracks)
		{
			crack.gameObject.SetActive(false);
		}

		StartCoroutine(FadeToBlack());
	}

	private IEnumerator FadeToBlack()
	{
		blackScreen.color = new Color(0, 0, 0, 1f);

		yield return new WaitForSeconds(1f);

		// Fade to black
		float elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			float alpha = elapsedTime / fadeDuration;
			deathText.color = new Color(deathText.color.r, deathText.color.g, deathText.color.b, alpha);
			yield return null;
		}

		// Show text for a while
		yield return new WaitForSeconds(2f);

		// Fade out
		elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			float alpha = 1 - (elapsedTime / fadeDuration);
			blackScreen.color = new Color(0, 0, 0, alpha);
			deathText.color = new Color(deathText.color.r, deathText.color.g, deathText.color.b, alpha);
			yield return null;
		}

		dead = false;
		hp = maxHp;
		damagedHp = hp;
	}



}
