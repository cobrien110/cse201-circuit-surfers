using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioManager : MonoBehaviour
{

    [SerializeField] public AudioSource audioSourceJump;
    [SerializeField] public AudioSource audioSourceFire;
    [SerializeField] public AudioSource audioSourceFireEmpty;
    [SerializeField] public AudioSource audioSourceReloadStart;
    [SerializeField] public AudioSource audioSourceReloadEnd;
    [SerializeField] public AudioSource audioSourceGrindrail;
    [SerializeField] public AudioSource audioSourceHurt;
    [SerializeField] public AudioSource audioSourceShieldBreak;
    [SerializeField] public AudioSource audioSourceShieldCharge;
    [SerializeField] public AudioSource audioSourceOverchargeFull;
    [SerializeField] public AudioSource audioSourceKeyGet;
    [SerializeField] public AudioSource audioSourceKeyFinal;
    [SerializeField] public AudioSource audioSourceDash;
    [SerializeField] public AudioSource audioSourceStartTimer;
    //[SerializeField] private AudioSource audioSourceSkating;
    public float volume = 1.0f;
    public Slider EffectVolume;
    [SerializeField] private TextMeshProUGUI Text;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(AudioSource audsource)
    {
        audsource.Play();
    }

    public void PlaySoundPitched(AudioSource audsource, float lowPitch, float highPitch)
    {
        // float initvol = audsource.volue
        // audsourcec.volume = initvol * volume %
        audsource.pitch = Random.Range(lowPitch, highPitch);
        audsource.Play();
        // audsou.vol = initvol
    }

    public void ChangeVolume()
    {
        EffectVolume.onValueChanged.AddListener((v) =>
        {
            volume = v;
            Text.text = "Effect Volume : " + volume + "%";
        });
    }
}
