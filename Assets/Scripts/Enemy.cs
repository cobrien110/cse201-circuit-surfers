using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float health = 50;
    [SerializeField] private float attackRange = 5;
    [SerializeField] private float attackSpeed = 3;
    [SerializeField] private float angerTime = 5;
    private float attackTimer = 0;
    public float angerTimer = 0;
    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioSource ausDamage;
    [SerializeField] private AudioSource ausAlert;
    [SerializeField] private ParticleSystem hurtParticles;
    public int attackBehavior = 0;
    //[SerializeField] private float rotSpeed = 5f;

    private GameObject player;
    private Quaternion startingRot;

    private bool inRange = false;
    private bool playAlertSound = false;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        startingRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        // Track and shoot player in range
        if (attackBehavior == 0)
        {
            Behavior0();
        }
        if (attackBehavior == 1)
        {
            Behavior1();
        }
    }

    public void Damage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        } else
        {
            if (attackBehavior == 0)
            {
                angerTimer = angerTime;
            }

            ausDamage.pitch = Random.Range(.85f, 1.15f);
            ausDamage.Play();
            hurtParticles.Play();
        }
    }

    // Track Player and Fire after timer
    private void Behavior0()
    {
        inRange = Vector3.Distance(transform.position, player.transform.position) <= attackRange;
        if (inRange || angerTimer > 0)
        {
            transform.LookAt(player.transform);
            attackTimer += 1 * Time.deltaTime;

            if (attackTimer >= attackSpeed)
            {
                RaycastHit objectHit;
                Vector3 fwd = transform.TransformDirection(Vector3.forward);
                if (Physics.Raycast(transform.position, fwd, out objectHit, attackRange * 2))
                {
                    if (objectHit.transform.gameObject.tag == "Player")
                    {
                        attackTimer = 0;
                        Instantiate(projectile, transform.position, transform.rotation);
                    }
                }
            }

            //Sound
            if (playAlertSound)
            {
                PlayAlertSound();
                playAlertSound = false;
            }
        }
        else
        {
            if (attackTimer > 0)
            {
                attackTimer -= 1 * Time.deltaTime;
            }

            Vector3 angles = transform.rotation.eulerAngles;
            //transform.Rotate(0f, Time.deltaTime * rotSpeed, 0f, Space.World);

            playAlertSound = true;
        }

        // Set anger state
        if (inRange)
        {
            angerTimer = angerTime;
        }
        if (angerTimer > 0)
        {
            angerTimer -= 1 * Time.deltaTime;
        }
    }

    // Dummy
    // Track Player but don't fire
    private void Behavior1()
    {
        if (inRange || angerTimer > 0)
        {
            transform.LookAt(player.transform);

            //Sound
            if (playAlertSound)
            {
                PlayAlertSound();
                playAlertSound = false;
            }
        }
        else
        {
            //transform.Rotate(0f, Time.deltaTime * rotSpeed, 0f, Space.World);

            playAlertSound = true;
        }
    }

    private void PlayAlertSound()
    {
        ausAlert.Play();
    }
}
