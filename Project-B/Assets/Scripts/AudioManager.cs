using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
  public string name;
  public AudioClip[] clips;
  public float minPitch = 0.9f;
  public float maxPitch = 1.1f;
}

public class AudioManager : MonoBehaviour
{
  public static AudioManager Instance { get; private set; }

  public Sound[] sounds;

  private AudioSource audioSource;

  void Awake()
  {
    Instance = this;
    audioSource = GetComponent<AudioSource>();
  }

  public void PlaySound(string soundName)
  {
    Debug.Log("Playing " + soundName);
    Sound sound = System.Array.Find(sounds, s => s.name == soundName);
    if (sound != null && sound.clips.Length > 0)
    {
      AudioClip clip = sound.clips[Random.Range(0, sound.clips.Length)];
      audioSource.pitch = Random.Range(sound.minPitch, sound.maxPitch);
      audioSource.PlayOneShot(clip);
    } else
		{
      Debug.Log("Couldnt found sound with name " + soundName);
		}
  }

  public AudioClip getFirstAudioClip(string soundName)
	{
    Sound sound = System.Array.Find(sounds, s => s.name == soundName);
    if (sound != null && sound.clips.Length > 0)
    {
      AudioClip clip = sound.clips[0];
      return clip;
    }
    else
    {
      Debug.Log("Couldnt found sound with name " + soundName);
      return null;
    }
  }
}