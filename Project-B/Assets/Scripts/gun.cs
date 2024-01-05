using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;
using UnityEngine.Events;
using System;

public enum GunType
{
  Minigun,
  Shotgun,
  Laser
}


public class gun : MonoBehaviour
{
  [Header("Gun objects")]
  public GunType currentGunType;
  public Transform origin;
  
  public GameObject minigun;
  public Transform minigunTip;
  public float minigunFireRate = 0.2f;

  public GameObject shotgun;
  public Transform shotgunTip;
  public float shotgunFireRate = 0.8f;

  public GameObject laser;
  public Transform laserTip;
  public float laserFireRate = 0.3f;

  [Header("Gun ammo")]
  public PlacePoint ammoCanPlacePoint; /// TODO: GUN SWAPPING, basically we check in the place point if he has Ammo script component, and then we change gun based o n the placed object. Mabe we can use the onplace event in code. https://earnestrobot.notion.site/Place-Points-e6361a414928450dbb53d76fd653cf9a. I would add the event just like onsqueeze here in code and check what tag the grabbable has.

  [Header("Gun controller")]
  public Transform gunAim;
  public Grabbable gunGrabbable;

  [Header("Shooting variables")]
  public float aimDistance = 15f; //distance where to aim

  public GameObject minigunBulletPrefab;
  public GameObject shotgunBulletPrefab;
  public GameObject laserBulletPrefab;

  public float angleLimitShooting = 70f;

  [Header("On shoot events (helpful for adding effects sounds etc)")]
  public UnityEvent OnShootMinigun;
  public UnityEvent OnShootShotgun;
  public UnityEvent OnShootLasergun;

  [Header("Animation")]
  public float angleLimitAnimation = 2f;
  public float rotationDampingAnimation = 0.2f;
  public AudioManager audioManager;

  private GameObject currentBulletPrefab;
  private Transform currentTip;
  private bool shooting = false;
  private bool shootingBefore = false; //for laser trigger
  private bool playedReloadShotgun = false; //for shotgun reload sound
  private float fireTimer = 0f;

  private AmmoCan currentAmmoCan;
  private GameObject currentGameObjectInReloadStation;

  void Start()
  {
    currentBulletPrefab = minigunBulletPrefab;
    currentTip = minigunTip;

    gunGrabbable.OnSqueezeEvent += HandleSqueeze;
    gunGrabbable.OnUnsqueezeEvent += HandleUnsqueeze;

    ammoCanPlacePoint.OnPlaceEvent += OnPlaceAmmo;
    ammoCanPlacePoint.OnRemoveEvent += OnRemoveAmmo;

    changeGun(currentGunType);
  }

	private void OnDestroy()
	{
    gunGrabbable.OnSqueezeEvent -= HandleSqueeze;
    gunGrabbable.OnUnsqueezeEvent -= HandleUnsqueeze;
    ammoCanPlacePoint.OnPlaceEvent -= OnPlaceAmmo;
    ammoCanPlacePoint.OnRemoveEvent -= OnRemoveAmmo;
  }

	void Update()
  {
    ///aim where gun controller is aiming
    RaycastHit hit;
    int layerMask = LayerMask.GetMask("Wall", "Enemy");
    Vector3 targetPoint;

    // Ray from the current position forward
    Ray ray = new Ray(gunAim.position, gunAim.forward);

    // Perform the Raycast
    if (Physics.Raycast(ray, out hit, aimDistance, layerMask))
    {
      // Ray hit an object, use the hit point
      targetPoint = hit.point;
    }
    else
    {
      // Ray didn't hit, use a point 20 meters away
      targetPoint = ray.origin + ray.direction * aimDistance;
    }

    //look at the point
    LookAtWithLimits(currentTip, targetPoint, angleLimitShooting, 0);
    //animation
    LookAtWithLimits(origin, targetPoint, angleLimitAnimation, rotationDampingAnimation);

    //shoot trigger
    switch (currentGunType)
		{
      case GunType.Minigun:
        if (shooting && fireTimer <= 0)
				{
          Shoot();
          fireTimer = minigunFireRate;
				}
        break;

      case GunType.Shotgun:
        //clicked early
        if (shooting && fireTimer > 0)
				{
          audioManager.PlaySound("ShotgunClick");
          fireTimer += 0.1f;
				}

        //reload sound
        if (audioManager.getFirstAudioClip("ShotgunReload") != null)
				{
          if (!playedReloadShotgun && fireTimer <= audioManager.getFirstAudioClip("ShotgunReload").length)
				  {
            audioManager.PlaySound("ShotgunReload");
            playedReloadShotgun = true;
				  }
				}
        

        if (shooting && fireTimer <= 0)
				{
          Shoot();
          fireTimer = shotgunFireRate;
				}
        shooting = false;
        break;

      case GunType.Laser:
        Debug.Log(shooting + " " + shootingBefore);
        if (!shooting && shootingBefore && fireTimer <= 0)
				{
          Shoot();
          fireTimer = laserFireRate;
				}
        shootingBefore = shooting;
        break;
		}
    fireTimer = Math.Max(0, fireTimer - Time.deltaTime);
  }
  
  public void changeGun(GunType gunToChange)
	{
    currentGunType = gunToChange;

    // Disable all guns
    minigun.SetActive(false);
    shotgun.SetActive(false);
    laser.SetActive(false);

    //set different models based on currentGunType
    switch (currentGunType)
    {
      case GunType.Minigun:
        minigun.SetActive(true);
        currentTip = minigunTip;
        currentBulletPrefab = minigunBulletPrefab;
        break;

      case GunType.Shotgun:
        shotgun.SetActive(true);
        currentTip = shotgunTip;
        currentBulletPrefab = shotgunBulletPrefab;
        break;

      case GunType.Laser:
        laser.SetActive(true);
        currentTip = laserTip;
        currentBulletPrefab = laserBulletPrefab;
        break;
    }

    //play sound
    audioManager.PlaySound("GunSwap");
  }

  void LookAtWithLimits(Transform objectToRotate, Vector3 targetPoint, float angleLimit, float rotationDamping)
  {
    // For this example i'll be using the root objects forward vector as what i want to be my reference constraint vector.
    // You might want to use something else though.
    Vector3 defaultFacing = transform.forward;

    // The direction from this transform, pointing at the look at target.
    Vector3 directionToLookAtTarget = targetPoint - objectToRotate.position;


    float angle = Vector3.Angle(directionToLookAtTarget, defaultFacing);

    // Since i'm just using the root objects forward vector as a constraint, i can just use its rotation as my default rotation instead of calculation a Quaternion.LookAt.
    Quaternion defaultRotation = transform.rotation;
    // The look at rotation to the target if it were completely unrestrained.
    Quaternion lookAtCompleteRotation = Quaternion.LookRotation(directionToLookAtTarget);

    Quaternion finalRotation = Quaternion.identity;

    // If the angle is greater than our limit, return a rotation that is in the direction of the lookAtCompleteRotation but is limited to the angle we chose as a limit.
    // Otherwise, if its within our limit, we just return the rotation as is.
    if (angle > angleLimit)
      finalRotation = Quaternion.Slerp(defaultRotation, lookAtCompleteRotation, angleLimit / angle);
    else
      finalRotation = lookAtCompleteRotation;

    //smooth or not smooth rotate
    if (rotationDamping != 0)
      objectToRotate.rotation = Quaternion.Slerp(objectToRotate.rotation, finalRotation, Time.deltaTime * rotationDamping);
    else
      objectToRotate.rotation = finalRotation;
  }

  private void HandleSqueeze(Hand hand, Grabbable grabbable)
  {
    shooting = true;
  }

  private void HandleUnsqueeze(Hand hand, Grabbable grabbable)
  {
    shooting = false;
  }


  public void Shoot()
	{
    if (currentAmmoCan != null)
		{
      //no bullets
      if (currentAmmoCan.currentAmmoCount <= 0)
			{
        audioManager.PlaySound("NoAmmoClick");
        return;
			}
      //little amount 10% of bullets make sound
      else if ((float)currentAmmoCan.currentAmmoCount <= (float)currentAmmoCan.ammoCount * 0.1f)
      {
        audioManager.PlaySound("LowAmmoClick");
			}

      switch (currentGunType)
      {
        case GunType.Minigun:
          Instantiate(currentBulletPrefab, currentTip.position, currentTip.rotation);
          OnShootMinigun?.Invoke();
          break;

        case GunType.Shotgun:
          playedReloadShotgun = false;
          Instantiate(currentBulletPrefab, currentTip.position, currentTip.rotation);
          OnShootShotgun?.Invoke();
          break;

        case GunType.Laser:
          Instantiate(currentBulletPrefab, currentTip.position, currentTip.rotation);
          OnShootLasergun?.Invoke();
          break;
      }

      currentAmmoCan.currentAmmoCount--;
    } else
		{
      Debug.Log("Attempting to shoot out " + currentGameObjectInReloadStation);
		}
  }

  private void OnPlaceAmmo(PlacePoint placePoint, Grabbable grab)
  {
    currentGameObjectInReloadStation = grab.gameObject;
    // Attempt to get the Ammo script from the GameObject of ammoPlace
    currentAmmoCan = grab.gameObject.GetComponent<AmmoCan>();

    // Check if the Ammo script is attached
    if (currentAmmoCan != null)
    {
      // Access the type property from the Ammo script
      GunType ammoType = currentAmmoCan.ammoType;
      Debug.Log("Ammo Type: " + ammoType);

      //change gun if ammo can is new
      if (currentGunType != currentAmmoCan.ammoType) changeGun(currentAmmoCan.ammoType);
    }
    else
    {
      Debug.Log("Ammo script not found on the GameObject placed in reload");
    }
  }
  private void OnRemoveAmmo(PlacePoint placePoint, Grabbable grab)
	{
    currentGameObjectInReloadStation = null;
    currentAmmoCan = null;
  }

}


