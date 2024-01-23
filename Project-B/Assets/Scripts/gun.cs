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
  public int bulletCount = 10;
  public float spreadAngle = 10f; // Total spread angle

  public GameObject laser;
  public Transform laserTip;
  public float laserMaxChargeTime = 1.0f;

  [Header("Gun ammo")]
  public PlacePoint ammoCanPlacePoint; /// TODO: GUN SWAPPING, basically we check in the place point if he has Ammo script component, and then we change gun based o n the placed object. Mabe we can use the onplace event in code. https://earnestrobot.notion.site/Place-Points-e6361a414928450dbb53d76fd653cf9a. I would add the event just like onsqueeze here in code and check what tag the grabbable has.

  [Header("Gun controller")]
  public Transform gunAim;
  public Grabbable gunGrabbable;

  [Header("Shooting variables")]
  public float aimDistance = 15f; //distance where to aim
  public float maxAimDistance = 50f;

  public GameObject minigunBulletPrefab;
  public GameObject shotgunBulletPrefab;
  public GameObject laserBulletPrefab;

  public float angleLimitShooting = 70f;

  [Header("On shoot events (helpful for adding effects sounds etc)")]
  public UnityEvent OnShootMinigun;
  public UnityEvent OnShootShotgun;
  public UnityEvent OnShootLasergun;
  public UnityEvent OnChargeLasergun;

  [Header("Animation")]
  public float angleLimitAnimation = 2f;
  public float rotationDampingAnimation = 0.2f;
  public AudioManager audioManager;
	public LineRenderer laserSightLineRenderer;

	private Transform currentTip;
  private bool shooting = false;
  private bool shootingBefore = false; //for laser trigger
  private bool playedReloadShotgun = false; //for shotgun reload sound
  private float fireTimer = 0f;

  private AmmoCan currentAmmoCan;
  private GameObject currentGameObjectInReloadStation;

  void Start()
  {
    currentTip = minigunTip;

    gunGrabbable.OnSqueezeEvent += HandleSqueeze;
    gunGrabbable.OnUnsqueezeEvent += HandleUnsqueeze;

    gunGrabbable.OnGrabEvent += HandleGrab;
    gunGrabbable.OnReleaseEvent += HandleRelease;

    ammoCanPlacePoint.OnPlaceEvent += OnPlaceAmmo;
    ammoCanPlacePoint.OnRemoveEvent += OnRemoveAmmo;

		laserSightLineRenderer.enabled = false;
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
    if (Physics.Raycast(ray, out hit, maxAimDistance, layerMask))
    {
      // Ray hit an object, use the hit point
      targetPoint = hit.point;
    }
    else
    {
      // Ray didn't hit, use a point 15 meters away
      targetPoint = ray.origin + ray.direction * aimDistance;
    }

		// Draw the line using LineRenderer
		laserSightLineRenderer.SetPosition(0, ray.origin);
		laserSightLineRenderer.SetPosition(1, targetPoint);

    //shooting = true;
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
        fireTimer = Math.Max(0, fireTimer - Time.deltaTime);
        break;

      case GunType.Shotgun:
        //reload sound
        if (audioManager.getFirstAudioClip("ShotgunReload") != null)
				{
          //clicked early
          if (shooting && fireTimer > audioManager.getFirstAudioClip("ShotgunReload").length)
          {
            audioManager.PlaySound("ShotgunClick");
            fireTimer += 0.1f;
          }

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
        fireTimer = Math.Max(0, fireTimer - Time.deltaTime);
        break;

      case GunType.Laser:
        //charging
        if (shooting && currentAmmoCan != null && currentAmmoCan.currentAmmoCount > 0)
				{
          OnChargeLasergun?.Invoke();

					if (!audioManager.IsSoundPlaying("LaserChargeUp"))
          {
            audioManager.PlaySound("LaserChargeUp");
          }
          fireTimer = Math.Min(laserMaxChargeTime, fireTimer + Time.deltaTime);
        }

        //shoot
        if (!shooting && shootingBefore)
        {
          audioManager.StopSound("LaserChargeUp");
          Shoot();
          fireTimer = 0;
        }
        shootingBefore = shooting;
        break;
		}
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
        break;

      case GunType.Shotgun:
        shotgun.SetActive(true);
        currentTip = shotgunTip;
        break;

      case GunType.Laser:
        laser.SetActive(true);
        currentTip = laserTip;
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

  private void HandleGrab(Hand hand, Grabbable grabbable)
  {
		laserSightLineRenderer.enabled = true;

	}

  private void HandleRelease(Hand hand, Grabbable grabbable)
  {
		laserSightLineRenderer.enabled = false;
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
      else if ((float)currentAmmoCan.currentAmmoCount <= (float)currentAmmoCan.ammoCount * 0.25f)
      {
        audioManager.PlaySound("LowAmmoClick");
			}

      switch (currentGunType)
      {
        case GunType.Minigun:
          Instantiate(minigunBulletPrefab, currentTip.position, currentTip.rotation);
          OnShootMinigun?.Invoke();
          break;

        case GunType.Shotgun:
          playedReloadShotgun = false;

          for (int i = 0; i < bulletCount; i++)
          {
            // Calculate random angle within the cone
            float horizontalAngle = UnityEngine.Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            float verticalAngle = UnityEngine.Random.Range(-spreadAngle / 2f, spreadAngle / 2f);

            // Convert angles to a direction vector
            Quaternion horizontalRotation = Quaternion.AngleAxis(horizontalAngle, currentTip.up);
            Quaternion verticalRotation = Quaternion.AngleAxis(verticalAngle, currentTip.right);
            Quaternion bulletRotation = currentTip.rotation * horizontalRotation * verticalRotation;

            // Instantiate the bullet
            Instantiate(shotgunBulletPrefab, currentTip.position, bulletRotation);
          }

          OnShootShotgun?.Invoke();
          break;


        case GunType.Laser:
          float strength = fireTimer / laserMaxChargeTime;
          GameObject laserBulletInstance = Instantiate(laserBulletPrefab, currentTip.position, currentTip.rotation);
          Laser bulletScript = laserBulletInstance.GetComponent<Laser>();

          if (bulletScript != null)
          {
            bulletScript.strength = strength;
          }

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


