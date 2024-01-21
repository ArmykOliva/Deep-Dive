using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backToIdle : MonoBehaviour
{
    private Animator lightAnimator;

    private void Start()
    {
        lightAnimator = gameObject.GetComponent<Animator>();
    }

    public void BackToIdle()
    {
        lightAnimator.SetTrigger("Idle");
    }
}
