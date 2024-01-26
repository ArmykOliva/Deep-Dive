using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{

    
    
    [Header("Progress (%)")]
    [SerializeField]
    [Range(0,100f)]
  public float progress = 0f;
  public waveSpawner waveswpawn;

  void Update()
  {
    float targetProgress = ((float)waveswpawn.currentWaveIndex / waveswpawn.waves.Count) *100;
    float progressChangeSpeed = CalculateProgressChangeSpeed(targetProgress);
    progress = Mathf.MoveTowards(progress, targetProgress, progressChangeSpeed * Time.deltaTime);
  }

  float CalculateProgressChangeSpeed(float targetProgress)
  {
    // Vypočítá rychlost změny progress, závislou na rozdílu mezi aktuálním a cílovým progress
    float progressDifference = Mathf.Abs(targetProgress - progress);
    return progressDifference * 0.05f; // someScalingFactor je proměnná, kterou můžete upravit
  }
  public float getProgress()
    {
    if (waveswpawn.waves.Count - 2 - waveswpawn.currentWaveIndex <= 0) return 1f; 
        return progress/100;
    }


    
}
