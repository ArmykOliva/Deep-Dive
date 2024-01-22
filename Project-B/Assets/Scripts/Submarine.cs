using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Submarine : MonoBehaviour
{
	public int hp = 100;
	public Animator lightAnimator;
	public Animator panelAnimator;

	public void TakeDamage(int damage)
	{
		hp -= damage;
		lightAnimator.SetTrigger("Fade");
		panelAnimator.SetTrigger("Fade");
	}

	
}
