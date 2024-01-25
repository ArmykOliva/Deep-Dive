using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static TreeEditor.TreeEditorHelper;

public class AmmoDispenser : MonoBehaviour
{
	[System.Serializable]
	public struct GunAmmoPair
	{
		public GunType gunType;
		public GameObject ammoPrefab;
	}

	[SerializeField]
	private List<GunAmmoPair> ammoList = new List<GunAmmoPair>();

	public Dictionary<GunType, GameObject> ammoPrefabs;


	public Transform ammoCanSpawnPoint;
	private Dictionary<GunType, int> oreCounts = new Dictionary<GunType, int>();

	// Map each OreType to a specific ammo prefab
	public UnityEvent onAmmoCanSpawn;
	public UnityEvent onOreCollect;

	private bool spawninAmmo = false;

	void Awake()
	{
		ammoPrefabs = new Dictionary<GunType, GameObject>();
		foreach (GunAmmoPair pair in ammoList)
		{
			if (!ammoPrefabs.ContainsKey(pair.gunType))
			{
				ammoPrefabs.Add(pair.gunType, pair.ammoPrefab);
			}
		}
	}

	private void Start()
	{
		// Initialize the oreCounts dictionary with all OreTypes
		foreach (GunType oreType in System.Enum.GetValues(typeof(GunType)))
		{
			oreCounts[oreType] = 0;
		}
	}

	public void Collect(GunType type, int amount)
	{
		if (oreCounts.ContainsKey(type))
		{
			oreCounts[type] += amount;
		}
		else
		{
			oreCounts[type] = amount;
		}
		onOreCollect?.Invoke();
	}

	public void SpawnAmmo()
	{
		if (spawninAmmo) return; spawninAmmo = true;
		StartCoroutine(SpawnAmmoIfNeeded());
	}

	private IEnumerator SpawnAmmoIfNeeded()
	{
		foreach (GunType type in System.Enum.GetValues(typeof(GunType)))
		{
			Debug.Log(oreCounts[type]);
			while (oreCounts[type] >= 10)
			{
				// Wait for 0.5 seconds before spawning the next ammo
				yield return new WaitForSeconds(0.5f);
				oreCounts[type] -= 10;
				onAmmoCanSpawn?.Invoke();
				if (ammoPrefabs.TryGetValue(type, out GameObject prefab))
				{
					Instantiate(prefab, ammoCanSpawnPoint.position, Quaternion.identity); // Spawn the specific ammo prefab
				}	
			}
		}
		spawninAmmo = false;
	}
}
