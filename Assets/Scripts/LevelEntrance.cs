using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntrance : MonoBehaviour
{
    public GameObject myResPoint;
    PlayerController player;
    GameObject cam;
    
    // Start is called before the first frame update
    void Start()
    {
        RespawnPoint rp = myResPoint.GetComponent<RespawnPoint>();
        rp.isActive = true;
        rp.isFirstPoint = true;
        rp.ActivateVisuals(true);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        player.myResPoint = myResPoint;
        player.transform.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
