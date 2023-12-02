using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSensitivity : MonoBehaviour
{
    public bool isX = true;
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _text;

    void Update()
    {
        if (isX)
        {
            _text.text = "X Sensitivity : " + _slider.value;
        }
        else
        {
            _text.text = "Y Sensitivity : " + _slider.value;
        }
    }
}
