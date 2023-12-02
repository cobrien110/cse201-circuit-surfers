using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExit : MonoBehaviour
{
    public GameObject portalClosed;
    public GameObject portalOpen;

    private GameObject lookTarget;
    // Start is called before the first frame update
    void Start()
    {
        lookTarget = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(lookTarget.transform);
    }

    public void PortalOpen(bool b)
    {
        if (b)
        {
            portalOpen.SetActive(true);
            portalClosed.SetActive(false);
        } else
        {
            portalOpen.SetActive(false);
            portalClosed.SetActive(true);
        }
    }
}
