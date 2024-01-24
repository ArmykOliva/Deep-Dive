using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public GameObject shield;
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
        yield return new WaitForSeconds(10.0f);
        shield.gameObject.SetActive(false);

    }
}
