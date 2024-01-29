using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FakeAIAnimationHandler : MonoBehaviour
{
    public ParticleSystem ps;

    IEnumerator explodeCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        ps.Play();
        //TODO:
        //Fade()
        //Reload
    }
}
