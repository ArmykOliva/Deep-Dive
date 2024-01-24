using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCan : MonoBehaviour
{
    public GunType ammoType;
    public int ammoCount = 50;

    //[HideInInspector] 
    public int currentAmmoCount;

    private Material instanceMaterial;
    private Renderer objectRenderer;
    private MaterialPropertyBlock propBlock;
    private Color originalEmissionColor;
    private float originalEmissionIntensity;

    private Renderer renderer;

    private void Start()
    {
        currentAmmoCount = ammoCount;

        // Create a new instance of the material
        renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            instanceMaterial = new Material(renderer.material);
            renderer.material = instanceMaterial;
        }

        propBlock = new MaterialPropertyBlock();
        originalEmissionColor = renderer.material.GetColor("_EmissionColor");

        originalEmissionIntensity = 0.4f;
    }

    void Update()
    {
        float normalizedAmmoCount = (float)currentAmmoCount / (float)ammoCount;
        renderer.material.SetColor("_EmissionColor",originalEmissionColor*normalizedAmmoCount);
    }
    

    void OnDestroy()
    {
        // Clean up the created material instance when the object is destroyed
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}