using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text AmmoCount;

    [SerializeField] private TMP_Text Timer;
    private Color timerBaseColor;
    [SerializeField] private TMP_Text KeysRemainingText;
    [SerializeField] private GameObject ScoreBox;
    [SerializeField] private GameObject PauseScreen;

    [SerializeField] private GameObject StatusBars;

    // Player health bars
    [SerializeField] private Image healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image shieldBar;
    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private Image overchargeBar;
    [SerializeField] private TMP_Text ammoCounter;
    [SerializeField] private Image shieldBreakWarning;
    //[SerializeField] private Transform timeBonusSpawnLocation;
    //[SerializeField] private GameObject timeBonusPrefab;
    float healthBarWidth;
    float shieldBarWidth;
    float overchargeBarHeight;

    public int gameMins = 0;
    public int gameSecs = 0;
    [SerializeField] private int lowTimeThreshold = 10;
    private float gameStartTime = 0;
    private int gameTime = 0;
    private int tempTime = 0;
    private int keysNeeded;

    private GameManager GM;
    private NetcodeUI nui;
    PlayerCam playcam;

    private bool isZero = false;

    // Start is called before the first frame update
    void Start()
    {
        GM = GameObject.Find("GameManager").GetComponent<GameManager>();
        nui = GameObject.Find("Menus").GetComponent<NetcodeUI>();
        playcam = GameObject.Find("Main Camera").GetComponent<PlayerCam>();

        gameMins = GM.levelMinutes;
        gameSecs = GM.levelSeconds;
        timerBaseColor = Timer.color;
        keysNeeded = GM.GetKeysNeeded();
        KeysRemainingText.text = "" + keysNeeded;

        //curAmmo = int.Parse(AmmoCount.text);
        //AmmoCount.text = "" + curAmmo;

        healthBarWidth = healthBar.rectTransform.rect.width;
        shieldBarWidth = shieldBar.rectTransform.rect.width;
        overchargeBarHeight = overchargeBar.rectTransform.rect.height;

        UpdateTimerText();
    }

    // Update is called once per frame
    void Update() {
        if (GM.gameplay)
        {
            ScoreBox.SetActive(true);
            StatusBars.SetActive(true);
        } else
        {
            ScoreBox.SetActive(false);
            StatusBars.SetActive(false);
        }

        if (GM.timerRunning)
        {
            UpdateTime();
            if (gameStartTime == 0)
            {
                Debug.Log(gameStartTime);
                gameStartTime = Time.time;
            }

            gameTime = (int)(Time.time - gameStartTime);

            //Text Update
            UpdateTimerText();
        }

        if (isZero)
        {
            Debug.Log("Level Failed");
            nui.SetLoseScreen(true);
            GM.gameplay = false;
            Time.timeScale = 0;
            playcam.SetLockCursor(false);
        }
    }

    private void UpdateTimerText()
    {
        if (gameSecs < 10)
        {
            Timer.text = gameMins + ":0" + gameSecs;
        }
        else
        {
            Timer.text = gameMins + ":" + gameSecs;
        }
        isZero = gameMins == 0 && gameSecs == 0;
        if (gameMins == 0 && gameSecs < lowTimeThreshold)
        {
            Timer.color = Color.red;
        }
        else
        {
            Timer.color = timerBaseColor;
        }
    }

    public void UpdateTime()
    {
        if (tempTime != gameTime && !isZero)
        {
            tempTime++;

            if (gameSecs == 0)
            {
                gameSecs = 59;
                gameMins--;
            }
            else
            {
                gameSecs--;
            }
        }

        UpdateTimerText();
    }

    public void AddTime(int timeBonus)
    {
        if (!isZero)
        {
            gameSecs += timeBonus;
            if (gameSecs >= 60)
            {
                gameSecs -= 60;
                gameMins += 1;
            }

        }
    }

    public void UpdateKeyText()
    {
        keysNeeded--;
        KeysRemainingText.text = "" + keysNeeded;

        if (keysNeeded == 0)
        {
            KeysRemainingText.text = "Exit Open!";
        }
    }

    public void UpdateGUIBars(float health, float healthMax, float shield, float shieldMax, float overcharge, float overchargeMax, int ammo)
    {
        RectTransform hb = healthBar.rectTransform;
        hb.sizeDelta = new Vector2(-(healthBarWidth - (health / healthMax) * healthBarWidth), 0);
        healthText.text = "" + Mathf.Floor(health);

        RectTransform sb = shieldBar.rectTransform;
        sb.sizeDelta = new Vector2(-(shieldBarWidth - (shield / shieldMax) * shieldBarWidth), 0);
        shieldText.text = "" + Mathf.Floor(shield);

        RectTransform ob = overchargeBar.rectTransform;
        ob.sizeDelta = new Vector2(0, -(overchargeBarHeight - (overcharge / overchargeMax) * overchargeBarHeight));
        if (overcharge >= overchargeMax / 2)
        {
            overchargeBar.color = new Color32(252, 232, 3, 255);
        }
        else
        {
            overchargeBar.color = new Color32(199, 196, 165, 255);
        }

        ammoCounter.text = "" + ammo;
    }

    public void SetShieldBreakWarningVisible(bool show)
    {
        Debug.Log("Setting shield break to " + show);
        shieldBreakWarning.gameObject.SetActive(show);
    }

    internal void SpawnTimeVisual()
    {
        //Instantiate(timeBonusPrefab, timeBonusSpawnLocation);
    }

    public void BankTime()
    {
        int newMins = gameMins + PlayerPrefs.GetInt("bankedMins");
        int newSecs = gameSecs + PlayerPrefs.GetInt("bankedSecs");
        if (newSecs >= 60)
        {
            newSecs -= 60;
            newMins++;
        }
        PlayerPrefs.SetInt("bankedSecs", newSecs);
        PlayerPrefs.SetInt("bankedMins", newMins);
        Debug.Log("Banked Minutes = " + newMins + ", " + "Banked Seconds = " + newSecs);
    }

    public void ResetBankedTime()
    {
        PlayerPrefs.SetInt("bankedSecs", 0);
        PlayerPrefs.SetInt("bankedMins", 0);
        Debug.Log("Reset Banked Time");
    }

    public void Pause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PauseScreen.SetActive(true);
    }

    public void Unpause()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PauseScreen.SetActive(false);
    }
}
