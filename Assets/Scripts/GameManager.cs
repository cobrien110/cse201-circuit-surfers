using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //int teamScore = 0;
    //int enemyScore = 0;
    public bool gameplay = false;
    public bool timerRunning = false;
    public int levelSeconds = 0;
    public int levelMinutes = 2;

    private UIManager UIM;
    private LevelExit levelExit;

    public string[] levelNames;
    Scene tutorialScene;

    // Start is called before the first frame update
    void Start()
    {
        UIM = GameObject.Find("UIManager").GetComponent<UIManager>();
        levelExit = GameObject.FindGameObjectWithTag("LevelExit").GetComponent<LevelExit>();
        Time.timeScale = 1;
        gameplay = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        tutorialScene = SceneManager.GetSceneByName("Tutorial");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetKeysNeeded()
    {
        GameObject[] keysInLevel = GameObject.FindGameObjectsWithTag("Key");
        return keysInLevel.Length;
    }

    public void CallPortalSwap(bool open)
    {
        levelExit.PortalOpen(open);
    }

    public void StartGame()
    {
        timerRunning = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        gameplay = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadLevel(int sceneIndex)
    {
        string sceneName = levelNames[sceneIndex];
        

        if (UIM != null && SceneManager.GetActiveScene() != tutorialScene)
        {
            UIM.BankTime();
        }

        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1;
        gameplay = true;

        if (sceneName.Equals("Tutorial"))
        {
            if (UIM != null)
            {
                UIM.ResetBankedTime();
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
