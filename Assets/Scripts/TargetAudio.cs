using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAudio : MonoBehaviour
{
    // Start is called before the first frame update
    private AudioSource aud;
    void Start()
    {
        aud = GetComponent<AudioSource>();
        aud.pitch = Random.Range(.9f, 1.1f);
        aud.Play();

        StartCoroutine(DestroyAfterTime(10));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator DestroyAfterTime(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(this.gameObject);
    }
}
