using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PathCreation;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
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
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashCooldown = 0.2f;
    [SerializeField] private float dashTime = 0.25f;
    [SerializeField] private bool dashFollowsCamera = true;
    [SerializeField] private float coyoteTime = 0.45f;
    [SerializeField] private float idleTime = 1.5f;

    [Header("Camera")]
    [SerializeField] private float cameraFOVAdjustSpeed = 3f;
    [SerializeField] private float cameraFOVMaxBonus = 12f;
    [SerializeField] private float cameraFOVMaxBonusRail = 15f;
    [SerializeField] private float cameraFOVBase = 90f;

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
    public int keysHeld = 0;
    public int keysNeeded = 0;
    public float coyoteTimer;
    public float idleTimer;

    [Header("Grind Rails")]
    [SerializeField] private PathCreator pathCreator;
    [SerializeField] private float railSpeed = 0.0f;
    [SerializeField] private float maxRailSpeed = 10.0f;
    [SerializeField] private float grindSpeedChangeRate = 0.1f;
    [SerializeField] private float distanceTravelled;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    public LayerMask groundLayer;
    private bool grounded;

    [Header("Object References")]
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private Transform eyePosition;
    [SerializeField] private Camera cam;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private GameObject impactEffectOvercharged;
    [SerializeField] private GameObject shotProjectilePrefab;
    public GameObject myResPoint;


    #endregion

    Rigidbody rb;

    float horiInput;
    float vertInput;
    Vector3 moveDir;
    bool onRail = false;
    bool startRail = true;
    bool chargeFull = false;
    bool chargeShot = false;
    float camFOV;
    float healthBarWidth;
    float shieldBarWidth;
    float overchargeBarHeight;
    bool isReloading = false;
    bool isDashing = false;
    bool canDash = true;
    bool canActivateCheckPoints = false;
    int ammo;
    bool paused = false;

    PlayerCam playcam;
    bool isDead = false;
    AudioManager am;
    private UIManager uim;
    private GameManager gm;
    private NetcodeUI nui;
    private Animator gunAnimator;
    

    // Start is called before the first frame update
    void Start()
    {
        // Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        uim = GameObject.Find("UIManager").GetComponent<UIManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        nui = GameObject.Find("Menus").GetComponent<NetcodeUI>();
        keysNeeded = gm.GetKeysNeeded();
        gunAnimator = GameObject.Find("Gun").GetComponent<Animator>();

        ResetVarsOnSpawn();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || !gm.gameplay)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        chargeFull = overcharge == overchargeMax ? true : false;
        GetInput();
        ControlSpeed();
        CheckGrounded();
        CameraFOV();
        uim.UpdateGUIBars(health, healthMax, shield, shieldMax, overcharge, overchargeMax, ammo);

        playerRotation = transform.rotation.eulerAngles.y;

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

        if (Input.GetKeyDown(KeyCode.Escape) && !paused && gm.gameplay != false)
        {
            uim.Pause();
            paused = true;
        } else if (Input.GetKeyDown(KeyCode.Escape) && paused)
        {
            
            uim.Unpause();
            paused = false;
        }

        UpdateCamera();
    }

    private void FixedUpdate()
    {
        if (isDead || !gm.gameplay) return;

        Move();
        MoveOnRail();
    }

    private void GetInput()
    {
        // Movement input
        horiInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");
        // Idle charge loss
        if (Math.Abs(horiInput) < 0.5 && Math.Abs(vertInput) < 0.5 && velocity < moveSpeed / 4)
        {
            if (idleTimer > 0 && !onRail && grounded)
            {
                idleTimer -= Time.deltaTime;
                gunAnimator.SetBool("isWalking", false);
            } else
            {
                if (overcharge > 0 && grounded)
                {
                    overcharge += -chargeRate/8;
                    overcharge = Mathf.Clamp(overcharge, 0, overchargeMax);
                }   
            }
        } else
        {
            idleTimer = idleTime;
            gunAnimator.SetBool("isWalking", true);
        }

        // Jump
        if (Input.GetKey(KeyCode.Space) && canJump && (grounded || coyoteTimer > 0))
        {
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // toggle sprint
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
            gunAnimator.SetBool("isSprinting", true);
        }
        else
        {
            isSprinting = false;
            gunAnimator.SetBool("isSprinting", false);
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

        // Dash
        if (Input.GetButtonDown("Fire2") && canDash && overcharge >= overchargeMax/2)
        {
            StartCoroutine(Dash());
        }
    }

    private void JumpOffGrindRail()
    {
        onRail = false;
        distanceTravelled = 0;
        startRail = true;
        am.audioSourceGrindrail.Stop();
        pathCreator = null;

        // Get direction
        moveDir = transform.forward * vertInput + transform.right * horiInput;
        rb.velocity = new Vector3(Mathf.Abs(railSpeed) * moveDir.x, 0f, Mathf.Abs(railSpeed) * moveDir.y);
        Jump();

        gunAnimator.SetBool("isGrinding", false);
    }

    private void CameraFOV()
    {
        if (!onRail && !isDashing)
        {
            if (isSprinting)
            {
                cam.fieldOfView += cameraFOVAdjustSpeed * Time.deltaTime;
            }
            else
            {
                cam.fieldOfView -= cameraFOVAdjustSpeed * Time.deltaTime;
                
            }
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, cameraFOVBase, cameraFOVBase + cameraFOVMaxBonus);
        } else
        {
            if (Mathf.Abs(railSpeed) >= maxRailSpeed / 2)
            {
                cam.fieldOfView += cameraFOVAdjustSpeed * Time.deltaTime;
            }
            else
            {
                cam.fieldOfView -= cameraFOVAdjustSpeed * Time.deltaTime;
            }
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, cameraFOVBase, cameraFOVBase + cameraFOVMaxBonusRail);
        }

        if (isDashing)
        {
            cam.fieldOfView += cameraFOVAdjustSpeed * 2 * Time.deltaTime;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, cameraFOVBase, cameraFOVBase + cameraFOVMaxBonusRail);
        }

        if (cam.fieldOfView > cameraFOVBase + (cameraFOVMaxBonus * 2/3))
        {
            if(!playcam.speedLines.isPlaying)
            {
                playcam.speedLines.Play();
            }
        } else
        {
            if (playcam.speedLines.isPlaying)
            {
                playcam.speedLines.Stop();
            }
        }
    }

    private void UpdateCamera()
    {
        cameraHolder.transform.position = eyePosition.transform.position;
    }

    private void CheckGrounded()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * .5f + 0.2f, groundLayer);
        if (!grounded)
        {
            if (coyoteTimer > 0)
            {
                coyoteTimer -= Time.deltaTime;
            }
        } else
        {
            coyoteTimer = coyoteTime;
        }
        gunAnimator.SetBool("isAirborn", !grounded);
    }

    private void Move()
    {
        if (!onRail && !isDashing)
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
        if (onRail && pathCreator != null && !isDashing)
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
            Vector3 pathV = pathCreator.path.GetPointAtDistance(distanceTravelled);
            transform.position = new Vector3(pathV.x, pathV.y + 1, pathV.z);

            // Charge shields and overcharge
            if (shield < shieldMax && !shieldBroken)
            {
                shield += chargeRate;
                shield = Mathf.Clamp(shield, 0, shieldMax);
                if (!am.audioSourceShieldCharge.isPlaying)
                {
                    am.audioSourceShieldCharge.Play();
                }
                uim.UpdateGUIBars(health, healthMax, shield, shieldMax, overcharge, overchargeMax, ammo);

            } else if (!shieldBroken)
            {
                overcharge += chargeRate;
                overcharge = Mathf.Clamp(overcharge, 0, overchargeMax);

                if (overcharge == overchargeMax && !chargeFull)
                {
                    PlayChargeSound();
                }
            }
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        overcharge -= overchargeMax / 2;

        if (onRail)
        {
            onRail = false;
            pathCreator = null;
            distanceTravelled = 0;
            startRail = true;
            am.audioSourceGrindrail.Stop();
            gunAnimator.SetBool("isGrinding", false);
        }

        Vector3 dir;
        if (dashFollowsCamera)
        {
            dir = playcam.transform.forward.normalized;
        } else
        {
            dir = transform.forward.normalized;
        }

        am.PlaySoundPitched(am.audioSourceDash, 0.85f, 1.15f);
        rb.drag = sprintDrag;
        rb.velocity = new Vector3(dir.x * dashSpeed, (dir.y * dashSpeed) / 2, dir.z * dashSpeed);
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        rb.drag = groundDrag;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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

        chargeShot = overcharge == overchargeMax ? true : false;
        ammo--;
        muzzleFlash.Play();
        am.PlaySoundPitched(am.audioSourceFire, 0.85f, 1.15f);
        gunAnimator.Play("Shoot");
        //PlayerProjectile myShot = Instantiate(shotProjectilePrefab, muzzleFlash.transform.position, playcam.transform.rotation).GetComponent<PlayerProjectile>();

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxRange)) {
            Debug.Log("You hit " + hit.transform.name + " at range " + String.Format("{0:N2}", hit.distance));
            //myShot.SetRotation(hit.point);

            // Hitting enemy
            Enemy enem = hit.transform.GetComponent<Enemy>();
            if (enem != null)
            {
                if (chargeShot)
                {
                    enem.Damage(damage * 2);
                }
                else
                {
                    enem.Damage(damage);
                }
            }

            // Hitting, "Reflecting" enemy projectile
            EnemyProjectile enemP = hit.transform.GetComponent<EnemyProjectile>();
            if (enemP != null)
            {
                ReflectedProjectile rp = Instantiate(enemP.reflectedShotPrefab, enemP.transform.position, enemP.transform.rotation).GetComponent<ReflectedProjectile>();
                rp.speed = enemP.speed * 2;
                Destroy(enemP.gameObject);
            }

            // Shooting Timer Target
            if (hit.collider.gameObject.tag == "Target")
            {
                uim.AddTime(5);
                uim.SpawnTimeVisual();
                Target t = hit.collider.gameObject.GetComponent<Target>();
                Instantiate(t.destroyedAudioPrefab, t.gameObject.transform.position, Quaternion.identity);
                Instantiate(t.explosionPrefab, t.gameObject.transform.position, Quaternion.identity);
                Destroy(hit.collider.gameObject);
            }

            // Shooting Switch Target
            if (hit.collider.gameObject.tag == "Switch")
            {
                uim.SpawnTimeVisual();
                Switch s = hit.collider.gameObject.GetComponent<Switch>();
                Instantiate(s.destroyedAudioPrefab, s.gameObject.transform.position, Quaternion.identity);
                Instantiate(s.explosionPrefab, s.gameObject.transform.position, Quaternion.identity);
                s.StartPlatformSequence();
                s.HideVisual();
            }

            // Ground Hit Effects
            if (chargeShot)
            {
                Instantiate(impactEffectOvercharged, hit.point, Quaternion.LookRotation(hit.normal));
            } else
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }

        }

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
        gunAnimator.SetBool("isReloading", true);
        yield return new WaitForSeconds(reloadTime);
        ammo = ammoMax;
        isReloading = false;
        gunAnimator.SetBool("isReloading", false);
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
        if (isDashing)
        {
            return;
        }

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

    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Road" && !onRail)
        {
            // Collide with trigger, get the rail and the point of collision
            //pathCreator = other.GetComponent<GetPath>().Path();
            //collisionPoint = other.ClosestPoint(transform.position);
        }
    }
    */

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Key")
        {
            keysHeld++;

            Key k = other.gameObject.GetComponent<Key>();
            Instantiate(k.explosionPrefab, k.transform.position, Quaternion.identity);
            overcharge += k.overchargeBonus;
            overcharge = Mathf.Clamp(overcharge, 0, overchargeMax);
            Destroy(other.gameObject);
            uim.UpdateKeyText();

            if (keysHeld == keysNeeded)
            {
                am.PlaySound(am.audioSourceKeyFinal);
                gm.CallPortalSwap(true);
            } else
            {
                am.PlaySoundPitched(am.audioSourceKeyGet, 0.975f, 1.025f);
            }
        }

        if (other.gameObject.tag == "LevelExit" && keysHeld >= keysNeeded)
        {
            Debug.Log("Level Complete");
            nui.SetWinScreen(true, uim.gameMins, uim.gameSecs);
            gm.gameplay = false;
            Time.timeScale = 0;
            playcam.SetLockCursor(false);
        }

        if (other.gameObject.tag == "Respawner" && canActivateCheckPoints)
        {
            Debug.Log("CHECKPOINT TRIGGER");
            RespawnPoint rp;
            if (myResPoint != null && !myResPoint.Equals(other.gameObject))
            {
                rp = myResPoint.GetComponent<RespawnPoint>();
                rp.ActivateVisuals(false);
            } 
            myResPoint = other.gameObject;
            rp = other.gameObject.GetComponent<RespawnPoint>();
            Debug.Log(rp.isActive);
            if (!rp.isActive)
            {
                rp.ActivateVisuals(true);
            } 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Respawner")
        {
            if (!canActivateCheckPoints)
            {
                canActivateCheckPoints = true;
            }
        }
        if (other.gameObject.tag == "StartZone" && !gm.timerRunning)
        {
            gm.StartGame();
            other.gameObject.SetActive(false);
            am.PlaySound(am.audioSourceStartTimer);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Road" && !onRail)
        {
            Debug.Log("Colliding with Rail");
            if (pathCreator == null)
            {
                // Collide with solid mesh, set transform location
                
                onRail = true;
                pathCreator = collision.gameObject.GetComponent<GetPath>().Path();
                collisionPoint = collision.contacts[0].point;
                transform.position = new Vector3(collisionPoint.x, collisionPoint.y + 1, collisionPoint.z);
                //Debug.Log(collisionPoint);

                if (isDashing)
                {
                    isDashing = false;
                    StopCoroutine(Dash());
                }
            }
        }

        
    }

    public void Damage(float amount)
    {
        if (isDead) return;

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
        cam.transform.SetParent(null);
        
        playcam.HideWeapon();

        isDead = true;

        StopAllCoroutines();
        StartCoroutine(Spawn());
    }

    public IEnumerator Spawn()
    {
        yield return new WaitForSeconds(respawnTime);
        isDead = false;

        ResetVarsOnSpawn();

        // Get respawn point
        /*
        GameObject[] respawnPoints = (GameObject[])UnityEngine.GameObject.FindGameObjectsWithTag("Respawner");
        int randomRespawnInt = Random.Range(0, respawnPoints.Length - 1);
        transform.position = respawnPoints[randomRespawnInt].transform.position;
        playcam.GetRespawnRotation(respawnPoints[randomRespawnInt]);
        */
        transform.position = myResPoint.transform.position;
        playcam.GetRespawnRotation(myResPoint);
        RespawnPoint rp = myResPoint.GetComponent<RespawnPoint>();
        rp.PlaySound();
    }

    private void ResetVarsOnSpawn()
    {
        // Reset vars
        onRail = false;
        startRail = true;
        isReloading = false;
        shieldBroken = false;
        isDashing = false;
        canDash = true;

        canJump = true;
        currentGroundDrag = groundDrag;
        velocity = rb.velocity.magnitude;
        currentMaxSpeed = moveSpeed;
        coyoteTimer = coyoteTime;
        idleTimer = idleTime;

        ammo = ammoMax;
        overcharge = 0f;

        health = healthMax;
        shield = shieldMax;
        uim.SetShieldBreakWarningVisible(false);

        // Camera
        cam.transform.SetParent(eyePosition);
        cam.transform.position = eyePosition.position;
        cam.transform.rotation = eyePosition.rotation;
        playcam = cam.GetComponent<PlayerCam>();
        playcam.ShowWeapon();
    }

    private IEnumerator ResetShield()
    {
        yield return new WaitForSeconds(shieldBreakCooldown);
        shieldBroken = false;
        uim.SetShieldBreakWarningVisible(false);
        shield = 1f;
    }

}
