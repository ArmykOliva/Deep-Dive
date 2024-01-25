using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
    public float timeDuration;

    private float time;
    
    
    [Header("Progress (%)")]
    [SerializeField]
    [Range(0,100f)]
    private float progress;
    
    
    // Start is called before the first frame update
    void Start()
    {
        time = timeDuration;
    }

    // Update is called once per frame
    void Update()
    {
        /*progress = (float) ((timeDuration - time) / timeDuration)*100;
        if (time > 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            //Load Scene
        }*/
    }

    public float getProgress()
    {
        return progress/100;
    }

    public float getTime()
    {
        return timeDuration;
    }


    
}
