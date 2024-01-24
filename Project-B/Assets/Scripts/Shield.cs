using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public GameObject shield;
    [SerializeField] private float shieldDuration=10.0f;
    private void Start()
    {
        shield.gameObject.SetActive(false);
    }

    public void ShieldUp()
    {
        StartCoroutine("shieldUp");

    }

    IEnumerator shieldUp()
    {
        shield.gameObject.SetActive(true);
        yield return new WaitForSeconds(shieldDuration);
        shield.gameObject.SetActive(false);

    }
}
