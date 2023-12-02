using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public GameObject explosionPrefab;
    [SerializeField] private float attractionRange = 4f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float speedGainRate = 1f;
    [SerializeField] public float overchargeBonus = 25f;

    private bool isMoving = false;

    private GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");  
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, player.transform.position) <= attractionRange && !isMoving)
        {
            isMoving = true;
        }

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
            speed += speedGainRate * Time.deltaTime;
        }
    }
}
