using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EndingManager : MonoBehaviour
{
    private float timer;

    public float endBeginSeconds = 30f;

    public AIRobotController voicelines;

    public GameObject AI;

    public Transform AIShotPoint;

    public GameObject fakeAI;

  public shark shark;

    public Transform Core;
  public AudioSource music;

    [SerializeField] private float speed;

    private bool playing = false;

    public Collider collider;
    public Rigidbody rb;
    
    
    // Start is called before the first frame update
    void Awake()
    {
    

				Debug.Log("Ending Time!!!");
    }

    private void Start()
    {
        fakeAI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > endBeginSeconds && !playing)
        {
            timer = 0;
			music.Stop();
			voicelines.PlayVoiceLine("nice working with u");
            playing = true;
            
        }

        if (!AI.GetComponent<AudioSource>().isPlaying && playing)
        {
            AI.SetActive(false);
      shark.stopShootingFunction();
			foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
			{
				GameObject.Destroy(enemy);
			}
			
			StartCoroutine(ending());
        }
    }

    IEnumerator ending()
    {
        
        yield return new WaitForSeconds(3f);
        fakeAI.SetActive(true);
        
        
    }

}
