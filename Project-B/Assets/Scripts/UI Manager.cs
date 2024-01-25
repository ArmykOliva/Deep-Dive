using System;
using System.Collections;
using System.Collections.Generic;
using Autohand;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text distanceUI;
    
    public TMP_Text ammoUI;
    public TMP_Text gunNameUI;

    public gun Gun;

    public ProgressTracker progressTracker;
    
    public const float TOCORE = 5150f;

    // Update is called once per frame
    void Update()
    {
        UpdateDistanceUI();
        UpdateAmmoUI();
    }

    void UpdateDistanceUI()
    {
        distanceUI.text = (TOCORE-Math.Floor(TOCORE * progressTracker.getProgress())).ToString() + " km";
    }

    void UpdateAmmoUI()
    {
        ammoUI.text = Gun.getAmmoCount().ToString();
        gunNameUI.text = Gun.getGunType();
    }
    
}
