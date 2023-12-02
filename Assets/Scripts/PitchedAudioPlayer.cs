using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchedAudioPlayer : MonoBehaviour
{
    public float lowPitch = 0.85f;
    public float highPitch = 1.15f;
    // Start is called before the first frame update
    void Start()
    {
        AudioSource aus = GetComponent<AudioSource>();
        aus.pitch = Random.Range(lowPitch, highPitch);
        aus.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
