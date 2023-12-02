using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public bool isActive = false;
    public bool isFirstPoint = false;
    public GameObject ActiveObjects;
    public GameObject UnactiveObjects;
    public AudioSource aus;
    public GameObject editorArrow;

    // Start is called before the first frame update
    void Start()
    {
        editorArrow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateVisuals(bool b)
    {
        UnactiveObjects.SetActive(!b);
        ActiveObjects.SetActive(b);
        if (b)
        {
            if (!isFirstPoint)
            {
                PlaySound();
            }
            isActive = true;
            isFirstPoint = false;
        } else
        {
            isActive = false;
        }
    }

    public void PlaySound()
    {
        aus.Play();
    }
}
