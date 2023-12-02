using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCam : MonoBehaviour
{
    // Camera Variables
    public float xSensitivity = 50f;
    public float ySensitivity = 50f;

    public Transform orientation;
    public GameObject weapon;
    public GameObject nonScoreUI;
    public GameObject speedLinesObject;
    public ParticleSystem speedLines;

    float xRotation;
    float yRotation;
    public float returnX;
    public float returnY;

    public Slider xSlider;
    public Slider ySlider;

    public bool lockCursor = true;
    GameObject levelEntrance;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        speedLines = speedLinesObject.GetComponent<ParticleSystem>();
        speedLines.Stop();

        levelEntrance = GameObject.FindGameObjectWithTag("LevelEntrance");
        GetRespawnRotation(levelEntrance);
    }

    // Update is called once per frame
    void Update()
    {
        if (orientation == null) return;

        // Rotate
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0); ;
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        if (gm != null && !gm.gameplay) return;

        // Get mouse input
        float MouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * xSensitivity;
        float MouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * ySensitivity;


        yRotation += MouseX;
        xRotation -= MouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        
    }

    public void HideWeapon()
    {
        weapon.SetActive(false);
        //nonScoreUI.SetActive(false);
    }

    public void ShowWeapon()
    {
        weapon.SetActive(true);
        nonScoreUI.SetActive(true);
    }

    public void GetRespawnRotation(GameObject resPoint)
    {
        yRotation = resPoint.transform.rotation.eulerAngles.y;
        xRotation = resPoint.transform.rotation.x;
        Debug.Log("X1: " + xRotation + " Y1: " + yRotation + " X2: " + resPoint.transform.rotation.x + " Y2: " + resPoint.transform.rotation.y);
    }

    public void changeXSensitivity()
    {
        xSlider.onValueChanged.AddListener((v) =>
        {
            xSensitivity = v;
        });
    }

    public void changeYSensitivity()
    {
        ySlider.onValueChanged.AddListener((v) =>
        {
            ySensitivity = v;
        });
    }

    public void SetLockCursor(bool state)
    {
        if (state)
        {
            lockCursor = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else if (!state)
        {
            lockCursor = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
