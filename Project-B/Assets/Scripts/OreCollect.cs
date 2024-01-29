using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OreCollect : MonoBehaviour
{
	public UnityEvent OnCollect;
	public AmmoDispenser ammoDispenser;

	public void Collect(GunType type, int number)
	{
		// Logic to handle ore collection
		Debug.Log($"Collected {number} of {type}");
		OnCollect?.Invoke();

		if (ammoDispenser != null)
		{
			ammoDispenser.Collect(type, number);
		}
	}
}
