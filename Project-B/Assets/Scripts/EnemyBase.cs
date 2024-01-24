using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
  public int hp = 100;
  public int damage = 10; //10 damage = 1 broken thing;
  public Material flashMaterial;
  public Texture damageTexture;
  public EnemyBorder enemyBorder; // the border where the enemy will be moving most of the time
  public EnemyBorder enemyBorderSubmarine; // the border where the enemy will be attacking (shooting, biting, charging...)


  [Header("Useful events (helpful for adding effects sounds etc)")]
  public UnityEvent OnHit;
  public UnityEvent OnDie;
  public UnityEvent OnAttack;
  
  protected Renderer rend;
	protected Material normalMaterial;
  private float flashDuration = 0.1f; // Duration of the flash effect
  private Texture originalTexture;

	void Start()
  {
    rend = GetComponent<Renderer>();
    if (rend != null)
    {
      normalMaterial = rend.material; // Set normalMaterial to the current material

      if (normalMaterial.name == "Tropical Fish (Instance)")
			{
        originalTexture = normalMaterial.GetTexture("_Atlas");
      }
    }
  }

  public virtual void TakeDamage(int bulletDamage)
  {
    OnHit?.Invoke();
    hp -= bulletDamage;
    if (rend != null && flashMaterial != null)
    {
      Flash();
    }

    if (hp <= 0)
    {
      Die();
    }
  }

  private void Flash()
  {
    if (rend != null && flashMaterial != null)
    {
      StartCoroutine(FlashRoutine());
    }
  }

  private IEnumerator FlashRoutine()
  {
    //for fishes
    if (normalMaterial.name == "Tropical Fish (Instance)")
		{
      normalMaterial.SetTexture("_Atlas", damageTexture);
      yield return new WaitForSeconds(flashDuration);
      normalMaterial.SetTexture("_Atlas", originalTexture);
    }
    //for normal materials
    else if (normalMaterial != null)
    {
      rend.material = flashMaterial;
      yield return new WaitForSeconds(flashDuration);
      rend.material = normalMaterial;
    }
    
  }

  protected virtual void Die()
  {
    OnDie?.Invoke();
    // Implement death logic here
    Destroy(gameObject);
  }
}