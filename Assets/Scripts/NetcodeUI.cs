using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class NetcodeUI : MonoBehaviour
{
    [SerializeField] GameObject WinScreen;
    [SerializeField] GameObject LoseScreen;
    [SerializeField] TMP_Text TimeRemaining;

    private GameManager GM;

    // Start is called before the first frame update
    void Start()
    {
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetWinScreen(bool state, int minutes, int seconds)
    {
        WinScreen.SetActive(state);
        if (seconds < 10)
        {
            TimeRemaining.text = "Time Remaining - " + minutes + ":0" + seconds;
        } else
        {
            TimeRemaining.text = "Time Remaining - " + minutes + ":" + seconds;
        }
    }

    public void SetLoseScreen(bool state)
    {
        LoseScreen.SetActive(state);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
