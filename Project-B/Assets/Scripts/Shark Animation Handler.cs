using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkAnimationHandler : MonoBehaviour
{
    public void CloseMouth()
    {
        this.GetComponent<Animator>().SetTrigger("CloseMouth");
    }

    public void OnOpenMouth()
    {
        
    }
}
