using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCan : MonoBehaviour
{
	public GunType ammoType;
	public int ammoCount = 50;

  [HideInInspector]
  public int currentAmmoCount;

  private Material instanceMaterial;
	private Renderer objectRenderer;
	private MaterialPropertyBlock propBlock;
	private Color originalEmissionColor;
	private float originalEmissionIntensity;

	private void Start()
	{
    currentAmmoCount = ammoCount;

    // Create a new instance of the material
    Renderer renderer = GetComponent<Renderer>();
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
		UpdateEmissionIntensity(normalizedAmmoCount);
  }

	void UpdateEmissionIntensity(float normalizedValue)
	{
		// Calculate new emission color based on the normalized ammo count
		Color newEmissionColor = instanceMaterial.GetColor("_EmissionColor") * Mathf.Lerp(0, originalEmissionIntensity, normalizedValue);
		instanceMaterial.SetVector("_EmissionColor", new Vector4(0.8196f, 0.783f, 0) * -4.0f);

		// Apply the new emission color to the material instance
		/*instanceMaterial.SetColor("_EmissionColor", newEmissionColor);
		DynamicGI.SetEmissive(objectRenderer, newEmissionColor); // Update global illumination with the new emission*/
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
