using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using PathCreation;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerControllerNetwork : NetworkBehaviour
{
    //public static PlayerController Instance { get; private set; }
    int playerNumber;

    #region Editor Variables
    // Variables
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 0.5f;
    [SerializeField] private float decceleration = 0.5f;
    [SerializeField] private float sprintMultiplier = 1.35f;
    //[SerializeField] private float chargeMultiplier = 1.25f;
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float sprintDrag = 0.5f;
    [SerializeField] private float dragAdjustSpeed = 0.1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float sprintJumpMult = 1.1f;
    [SerializeField] private float jumpCooldown = 0.05f;
    [SerializeField] private float airMultiplier = 0.8f;

    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float maxRange = 100f;
    [SerializeField] private float health;
    [SerializeField] private float healthMax = 100f;
    [SerializeField] private float shield;
    [SerializeField] private float shieldMax = 50f;
    [SerializeField] private float shieldBreakCooldown = 5f;
    [SerializeField] private bool shieldBroken = false;
    [SerializeField] private float overcharge = 0f;
    [SerializeField] private float overchargeMax = 100f;
    [SerializeField] private float chargeRate = 1f;
    [SerializeField] private int ammoMax = 6;
    [SerializeField] private float reloadTime = 0.8f;
    [SerializeField] private float respawnTime = 1f;
    [SerializeField] private float cameraFOVBase = 90f;

    [Header("Tracking Vars DO NOT EDIT")]
    public bool canJump;
    public bool isSprinting;
    public float currentGroundDrag;
    public float velocity;
    public float currentMaxSpeed;
    public float percentOfSpeed = 0f;
    [SerializeField] private Vector3 collisionPoint;
    public float pathRotation;
    public float playerRotation;
    public bool grindingForwards = false;

    [Header("Grind Rails")]
    [SerializeField] private PathCreator pathCreator;
    [SerializeField] private float railSpeed = 0.0f;
    [SerializeField] private float maxRailSpeed = 10.0f;
    [SerializeField] private float grindSpeedChangeRate = 0.1f;
    [SerializeField] private float distanceTravelled;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    public LayerMask groundLayer;
    public bool grounded;

    [Header("Object References")]
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private Transform eyePosition;
    private Camera cam;
    private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject impactEffect;

    [SerializeField] private GameObject playerModel;
    private AudioManager am;
    private UIManager uim;
    private GameManager gm;
    private PlayerAnimator playerAnimator;

    #endregion

    Rigidbody rb;

    float horiInput;
    float vertInput;
    Vector3 moveDir;
    bool onRail = false;
    bool startRail = true;
    bool chargeFull = false;
    float camFOV;

    bool isReloading = false;
    int ammo;
    PlayerCam playcam;
    bool isDead = false;

    private void Start()
    {
        // Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        am = GameObject.Find("PlayerAudioSources").GetComponent<AudioManager>();
        playerAnimator = this.gameObject.GetComponent<PlayerAnimator>();
        uim = GameObject.Find("UIManager").GetComponent<UIManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (IsHost)
        {
            //gm.AddToIndex(this);
        }
        //playerNumber = gm.myPlayerId;

        if (IsOwner)
        {
            am.PlaySoundPitched(am.audioSourceOverchargeFull, 0.85f, 1.15f);
            cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            cameraHolder = cam.gameObject;
            muzzleFlash = GameObject.Find("MuzzleFlash").GetComponent<ParticleSystem>();
            playcam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerCam>();
            ResetVarsOnSpawn();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playcam.orientation = this.gameObject.transform;
            playerModel.SetActive(false);
            //uim.player = this;
            
            Debug.Log("My player number is " + playerNumber);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();
        if (isDead)
        {
            rb.velocity = new Vector3(0f,0f,0f);
            return;
        }
        playerRotation = transform.rotation.eulerAngles.y;

        if (!IsOwner)
        {
            return;
        }

        GetInput();
        ControlSpeed();
        CameraFOV();

        // Jump Off Grind rail
        if (Input.GetKey(KeyCode.Space) && onRail)
        {
            JumpOffGrindRail();
        }

        // Reset Rail Reference
        if (!onRail)
        {
            pathCreator = null;
        }

        UpdateCamera();
        uim.UpdateGUIBars(health, healthMax, shield, shieldMax, overcharge, overchargeMax, ammo);
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        //if (!IsOwner) return;

        Move();
        MoveOnRail();
    }

    private void GetInput()
    {
        horiInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");
        if (horiInput != 0 || vertInput != 0)
        {
            playerAnimator.setMoving(true);
        } else
        {
            playerAnimator.setMoving(false);
        }
        playerAnimator.setGrinding(onRail);

        // Jump
        if (Input.GetKey(KeyCode.Space) && grounded && canJump)
        {
            canJump = false;
            Jump();
            playerAnimator.triggerJump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // toggle sprint
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        // Shoot
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        // Reload
        if (Input.GetKey(KeyCode.R))
        {
            StartReload();
        }
    }

    private void JumpOffGrindRail()
    {
        onRail = false;
        distanceTravelled = 0;
        startRail = true;
        pathCreator = null;

        // Get direction
        moveDir = transform.forward * vertInput + transform.right * horiInput;
        rb.velocity = new Vector3(Mathf.Abs(railSpeed) * moveDir.x, 0f, Mathf.Abs(railSpeed) * moveDir.y);
        Jump();
        am.audioSourceGrindrail.Stop();
    }

    private void CameraFOV()
    {
        if (cam == null)
        {
            return;
        }
        if (!onRail)
        {
            if (velocity > moveSpeed)
            {
                cam.fieldOfView = cameraFOVBase + ((velocity - 10f) * 1.5f);
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, cameraFOVBase, cameraFOVBase + 15f);
            }
            else
            {
                cam.fieldOfView -= 0.5f * Time.deltaTime;
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, cameraFOVBase, cameraFOVBase + 15f);
            }
        } else
        {
            cam.fieldOfView = cameraFOVBase + ((Mathf.Abs(railSpeed) - 5f) * 1.5f);
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, cameraFOVBase, cameraFOVBase + 15f);
        }
    }

    private void UpdateCamera()
    {
        cameraHolder.transform.position = eyePosition.transform.position;
    }

    private void CheckGrounded()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * .5f + 0.2f, groundLayer);
    }

    private void Move()
    {
        if (!onRail)
        {
            velocity = rb.velocity.magnitude;
            // Drag
            float dragTarget = isSprinting ? sprintDrag : groundDrag;
            // Increment / Decrement drag between sprint states
            if (currentGroundDrag > dragTarget)
            {
                currentGroundDrag -= dragAdjustSpeed;
            }
            else if (currentGroundDrag < dragTarget)
            {
                currentGroundDrag += dragAdjustSpeed;
            }
            currentGroundDrag = Mathf.Clamp(currentGroundDrag, sprintDrag, groundDrag);
            // Set drag
            rb.drag = grounded ? currentGroundDrag : 0f;

            // Speed
            float speed = isSprinting ? moveSpeed : moveSpeed * sprintMultiplier;

            // Movement
            // Get direction
            moveDir = transform.forward * vertInput + transform.right * horiInput;
            // Small acceleration effect
            if (Mathf.Abs(horiInput) > 0 || Mathf.Abs(vertInput) > 0)
            {
                percentOfSpeed += acceleration;
            }
            else
            {
                percentOfSpeed -= decceleration;
            }
            percentOfSpeed = Mathf.Clamp(percentOfSpeed, 0, 1);
            // Air control
            float inAirMult = grounded ? 1f : airMultiplier;
            // Add force
            rb.AddForce(moveDir.normalized * speed * 10f * percentOfSpeed * inAirMult, ForceMode.Force);
        }
    }

    private void MoveOnRail()
    {
        if (onRail && pathCreator != null)
        {
            // Initialize Movement
            if (startRail)
            {
                distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(collisionPoint);
                // Get rotation of path
                pathRotation = pathCreator.path.GetRotationAtDistance(distanceTravelled).eulerAngles.y;

                // Get grind direction
                float angleDif = Mathf.Abs(playerRotation - pathRotation);
                grindingForwards = Mathf.Abs(playerRotation - pathRotation) <= 90f ? true : false;

                railSpeed = Mathf.Clamp(velocity, -maxRailSpeed, maxRailSpeed);
                railSpeed = grindingForwards ? railSpeed : -railSpeed;
                startRail = false;
                am.audioSourceGrindrail.Play();
            }
            // Keep moving
            else
            {
                // Get rotation of path
                pathRotation = pathCreator.path.GetRotationAtDistance(distanceTravelled).eulerAngles.y;

                // Get grind direction
                float angleDif = Mathf.Abs(playerRotation - pathRotation);
                grindingForwards = angleDif <= 90f || angleDif > 270f ? true : false;

                // Add to rail speed
                float speedChange = grindingForwards ? grindSpeedChangeRate : -grindSpeedChangeRate;
                if (Input.GetKey(KeyCode.W)) {
                    railSpeed += speedChange;

                } else if (Input.GetKey(KeyCode.S))
                {
                    railSpeed -= speedChange;

                }
                if (railSpeed > maxRailSpeed)
                {
                    railSpeed = maxRailSpeed;
                } else if (railSpeed < -maxRailSpeed)
                {
                    railSpeed = -maxRailSpeed;
                }
                distanceTravelled += railSpeed * Time.deltaTime;
            }
            // Update position on rail
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled);

            // Charge shields and overcharge
            if (shield < shieldMax && !shieldBroken)
            {
                shield += chargeRate;
                shield = Mathf.Clamp(shield, 0, shieldMax);
                if (!am.audioSourceShieldCharge.isPlaying)
                {
                    am.audioSourceShieldCharge.Play();
                }
                
            } else if (!shieldBroken)
            {
                overcharge += chargeRate;
                overcharge = Mathf.Clamp(overcharge, 0, overchargeMax);
                if (overcharge == overchargeMax && !chargeFull)
                {
                    chargeFull = true;
                    PlayChargeSound();
                }
            }
        }
    }

    private void PlayChargeSound()
    {
        am.PlaySoundPitched(am.audioSourceOverchargeFull, 0.85f, 1.15f);
    }

    

    private void Shoot()
    {
        if (ammo <= 0 || isReloading) {
            if (!isReloading)
            {
                am.PlaySoundPitched(am.audioSourceFireEmpty, 0.85f, 1.15f);
            }
            return; 
        }
        ammo--;
        muzzleFlash.Play();
        am.audioSourceFire.Stop();
        am.PlaySoundPitched(am.audioSourceFire, 0.85f, 1.15f);

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxRange)) {
            Debug.Log("You hit " + hit.transform.name + " at range " + String.Format("{0:N2}", hit.distance));

            // Test hitting enemy
            PlayerControllerNetwork target = hit.transform.GetComponent<PlayerControllerNetwork>();
            if (target != null)
            {
                int targetPlayerNumber = target.playerNumber;
                if (IsHost)
                {
                    DamageClientRPC(damage, targetPlayerNumber);
                } else
                {
                    //DamageServerRPC();
                }
                
            }

            if (IsHost)
            {
                SpawnHitMarkerClientRPC(hit.point, Quaternion.LookRotation(hit.normal));
            } else
            {
                SpawnHitMarkerServerRPC(hit.point, Quaternion.LookRotation(hit.normal));
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
            
        }

    }

    [ClientRpc]
    private void SpawnHitMarkerClientRPC(Vector3 place, Quaternion rot)
    {
        Instantiate(impactEffect, place, rot);
    }

    [ServerRpc]
    private void SpawnHitMarkerServerRPC(Vector3 place, Quaternion rot)
    {
        Instantiate(impactEffect, place, rot);
    }

    [ClientRpc]
    private void DamageClientRPC(float damage, int targetPlayerNumber)
    {
        // THIS DOES NOT WORK!!!
        
        Debug.Log("Sending Damage RPC. Target: " + targetPlayerNumber + ", Running code: " + playerNumber);
        if (playerNumber == targetPlayerNumber)
        {
            Debug.Log("Number match!" + IsOwner);
            Damage(damage);
        }
    }

    [ServerRpc]
    private void DamageServerRPC()
    {
    
    }

    private void StartReload()
    {
        if (!isReloading)
        {
            StartCoroutine(Reload());
            am.PlaySoundPitched(am.audioSourceReloadStart, 0.85f, 1.15f);
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        ammo = ammoMax;
        isReloading = false;
        am.PlaySoundPitched(am.audioSourceReloadEnd, 0.85f, 1.15f);
    }

    public int GetAmmo()
    {
        return ammo;
    }

    private void Jump()
    {
        // Reset vertical velocity
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        float currentJumpForce = isSprinting ? jumpForce * sprintJumpMult : jumpForce;
        rb.AddForce(transform.up * currentJumpForce, ForceMode.Impulse);

        if (!am.audioSourceJump.isPlaying) {
            am.PlaySoundPitched(am.audioSourceJump, 0.85f, 1.15f);
        }
    }

    private void ControlSpeed()
    {
        if (onRail)
        {
            rb.velocity = new Vector3(0f, 0f, 0f);
            return;
        }
        
        // Ramp up / ramp down max speed between sprint states
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float targetSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        if (grounded) // Only change speed if not jumping
        {
            if (currentMaxSpeed < targetSpeed)
            {
                currentMaxSpeed += acceleration;
            }
            else if (currentMaxSpeed > targetSpeed)
            {
                currentMaxSpeed -= decceleration;
            }
            currentMaxSpeed = Mathf.Clamp(currentMaxSpeed, moveSpeed, moveSpeed * sprintMultiplier);
        }

        // if moving faster than current max, slow down
        if (flatVelocity.magnitude > currentMaxSpeed)
        {
            Vector3 newVelocity = flatVelocity.normalized * currentMaxSpeed;
            rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
        }
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Road" && !onRail)
        {
            // Collide with trigger, get the rail and the point of collision
            pathCreator = other.GetComponent<GetPath>().Path();
            collisionPoint = other.ClosestPoint(transform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Road" && !onRail)
        {
            if (pathCreator != null)
            {
                // Collide with solid mesh, set transform location
                onRail = true;
                transform.position = collisionPoint;
            }
        }
    }

    public void Damage(float amount)
    {
        if (isDead || !IsOwner) return;
        Debug.Log("Running damage code (" + amount + ")");

        if (shield > 0) // Damage Shield
        {
            shield -= amount;
        } else if (health > 0) // Damage Health
        {
            health -= amount;
        }
        if (shield <= 0) // Breaking Shield
        {
            shield = 0;
            if (!shieldBroken)
            {
                StartCoroutine(ResetShield());
                am.PlaySoundPitched(am.audioSourceShieldBreak, 0.85f, 1.15f);
                shieldBroken = true;
                uim.SetShieldBreakWarningVisible(true);
            } else
            {
                StopCoroutine(ResetShield());
                //StartCoroutine(ResetShield());
            }
            
        }

        if (!am.audioSourceHurt.isPlaying)
        {
            am.PlaySoundPitched(am.audioSourceHurt, 0.85f, 1.15f);
        }
        

        // Player death
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        //cam.transform.SetParent(null);
        
        //playcam.HideWeapon();
        playerModel.SetActive(false);
        isDead = true;

        StopAllCoroutines();
        StartCoroutine(Spawn());
    }

    public IEnumerator Spawn()
    {
        yield return new WaitForSeconds(respawnTime);
        isDead = false;

        ResetVarsOnSpawn();
        am.PlaySoundPitched(am.audioSourceOverchargeFull, 0.85f, 1.15f);

        // Get respawn point
        if (IsOwner) {
            GameObject[] respawnPoints = (GameObject[])UnityEngine.GameObject.FindGameObjectsWithTag("Respawner");
            int randomRespawnInt = Random.Range(0, respawnPoints.Length - 1);
            transform.position = respawnPoints[randomRespawnInt].transform.position;
            playcam.GetRespawnRotation(respawnPoints[randomRespawnInt]);
        }
        
    }

    private void ResetVarsOnSpawn()
    {
        if (!IsOwner) return;
        
        // Reset vars
        onRail = false;
        startRail = true;
        chargeFull = false;
        isReloading = false;
        shieldBroken = false;

        canJump = true;
        currentGroundDrag = groundDrag;
        velocity = rb.velocity.magnitude;
        currentMaxSpeed = moveSpeed;

        ammo = ammoMax;
        overcharge = 0f;

        health = healthMax;
        shield = shieldMax;
        //shieldBreakWarning.enabled = false;

        // Camera
        //cam.transform.SetParent(camHolderPoint.transform);
        cam.transform.position = eyePosition.position;
        cam.transform.rotation = eyePosition.rotation;
        playcam = cam.GetComponent<PlayerCam>();
        playcam.ShowWeapon(); 

        am.PlaySoundPitched(am.audioSourceOverchargeFull, 0.85f, 1.15f);
    }

    private IEnumerator ResetShield()
    {
        yield return new WaitForSeconds(shieldBreakCooldown);
        shieldBroken = false;
        uim.SetShieldBreakWarningVisible(false);
        shield = 1f;
    }
}
