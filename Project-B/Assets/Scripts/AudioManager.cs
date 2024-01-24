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
  public float spatialBlend = 0.5f;
  public float minDistance = 0.7f;
  public float maxDisatance = 50.0f;
}

public class AudioManager : MonoBehaviour
{
  public static AudioManager Instance { get; private set; }

  public Sound[] sounds;
  
  private Dictionary<string, AudioSource> soundAudioSources = new Dictionary<string, AudioSource>();
  private Dictionary<string, Sound> soundDatas = new Dictionary<string,Sound>();

  void Awake()
  {
    Instance = this;
    foreach (var sound in sounds)
    {
      AudioSource source = gameObject.AddComponent<AudioSource>();
			source.spatialBlend = sound.spatialBlend; // Ensure the sound is fully 3D
			source.minDistance = sound.minDistance; // The distance within which the volume is at its maximum
			source.maxDistance = sound.maxDisatance; // The maximum distance at which the sound is still audible
			
      soundAudioSources[sound.name] = source;
      soundDatas[sound.name] = sound;
    }
  }

  public void PlaySound(string soundName)
  {
    if (soundAudioSources.TryGetValue(soundName, out AudioSource source))
    {
      Sound sound = soundDatas[soundName];
      AudioClip clip = sound.clips[Random.Range(0, sound.clips.Length)];
      source.pitch = Random.Range(sound.minPitch, sound.maxPitch);
      source.PlayOneShot(clip);
    }
    else
    {
      Debug.Log("Couldn't find sound with name " + soundName);
    }
  }

  public void StopSound(string soundName)
  {
    if (soundAudioSources.TryGetValue(soundName, out AudioSource source))
    {
      source.Stop();
    }
    else
    {
      Debug.Log("Couldn't find sound with name " + soundName);
    }
  }

  public AudioClip getFirstAudioClip(string soundName)
  {
    if (soundDatas.TryGetValue(soundName, out Sound sound))
    {
      return sound.clips[0];
    }
    else
    {
      Debug.Log("Couldn't find sound with name " + soundName);
      return null;
    }
  }

  public bool IsSoundPlaying(string soundName)
  {
    if (soundAudioSources.TryGetValue(soundName, out AudioSource source))
    {
      return source.isPlaying;
    }
    else
    {
      Debug.Log("Couldn't find sound with name " + soundName);
      return false;
    }
  }
}