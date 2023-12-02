using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCycleNoEmission : MonoBehaviour
{
    Material myMaterial;
    public Color startColor;
    public Color endColor;
    public float time = 1f;
    bool goingForward;
    bool isCycling;

    // Start is called before the first frame update
    void Start()
    {
        goingForward = true;
        isCycling = false;
        myMaterial = GetComponent<Renderer>().material;
    }


    // Update is called once per frame
    void Update()
    {
        if (!isCycling)
        {
            if (goingForward)
            {
                StartCoroutine(CycleMaterial(startColor, endColor, time, myMaterial));
            }
            else
            {
                StartCoroutine(CycleMaterial(endColor, startColor, time, myMaterial));
            }
        }
    }

    private IEnumerator CycleMaterial(Color startColor, Color endColor, float cycleTime, Material mat)
    {
        isCycling = true;
        float currentTime = 0;
        while (currentTime < cycleTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / cycleTime;
            Color currentColor = Color.Lerp(startColor, endColor, t);
            mat.color = currentColor;
            yield return null;
        }
        isCycling = false;
        goingForward = !goingForward;
    }
}
