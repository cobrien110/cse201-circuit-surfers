using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffUI : MonoBehaviour
{

    public GameObject[] UIElements;
    private bool paused;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !paused)
        {
            paused = true;
            for (int i = 0; i < UIElements.Length; i++)
            {
                UIElements[i].SetActive(false);
            }
        } else if (Input.GetKeyDown(KeyCode.Escape) && paused) {
            paused = false;
            for (int i = 0; i < UIElements.Length; i++)
            {
                UIElements[i].SetActive(true);
            }
        }
    }
}
