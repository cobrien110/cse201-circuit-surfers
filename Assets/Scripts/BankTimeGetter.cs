using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BankTimeGetter : MonoBehaviour
{

    [SerializeField] private TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        int secs = PlayerPrefs.GetInt("bankedSecs");
        int mins = PlayerPrefs.GetInt("bankedMins");
        if (secs < 10)
        {
            text.text = mins + ":0" + secs;
        } else
        {
            text.text = mins + ":" + secs;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
