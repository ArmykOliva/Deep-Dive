using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Submarine : MonoBehaviour
{
	public int hp = 100;

	public void TakeDamage(int damage)
	{
		hp -= damage;
	}
}
