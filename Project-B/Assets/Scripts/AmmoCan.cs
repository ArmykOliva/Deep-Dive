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
  private float saturationLevel = 1.0f;

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
  }

  void Update()
  {
    float normalizedAmmoCount = (float)currentAmmoCount / (float)ammoCount;
    saturationLevel = Mathf.Clamp01(Mathf.Pow(normalizedAmmoCount, 0.2f)); // Adjust this exponent to change the curve

    if (instanceMaterial != null)
    {
      Color originalColor = instanceMaterial.color;

      // Convert to HSV
      Color.RGBToHSV(originalColor, out float H, out float S, out float V);

      // Adjust saturation
      S *= saturationLevel;

      // Convert back to RGB
      instanceMaterial.color = Color.HSVToRGB(H, S, V);
    }
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
