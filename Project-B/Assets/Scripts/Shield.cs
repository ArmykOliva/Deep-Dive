using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Shield : MonoBehaviour
{
	public GameObject shield;
	public UnityEvent onShieldUp;
	public UnityEvent onShieldDown;
	public UnityEvent onShieldRecharge;
	[SerializeField] private float shieldDuration = 10.0f;
	[SerializeField] private float shieldRechargeDuration = 30f;

	private bool isRecharging = false;

	private void Start()
	{
		shield.gameObject.SetActive(false);
	}

	public void ShieldUp()
	{
		// Only allow the shield to go up if it is not currently recharging
		if (!isRecharging)
		{
			StartCoroutine(ShieldUpRoutine());
			StartCoroutine(ShieldRechargeRoutine());
		}
	}

	IEnumerator ShieldUpRoutine()
	{
		shield.gameObject.SetActive(true);
		onShieldUp?.Invoke();

		// Shield is up for the duration
		yield return new WaitForSeconds(shieldDuration);

		shield.gameObject.SetActive(false);
		onShieldDown?.Invoke();
	}

	IEnumerator ShieldRechargeRoutine()
	{
		isRecharging = true;

		// Wait for the recharge duration
		yield return new WaitForSeconds(shieldRechargeDuration);
		onShieldRecharge.Invoke();
		isRecharging = false;
	}
}
