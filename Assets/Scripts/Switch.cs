using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField] public GameObject destroyedAudioPrefab;
    [SerializeField] public GameObject explosionPrefab;
    [SerializeField] public GameObject previewPrefab;
    [SerializeField] public GameObject[] thingsToActivate;
    private GameObject[] previewObjects;
    [SerializeField] private GameObject spawnEffectPrefab;

    private int arLen;
    private int arCount = 0;
    public float spawnDelay = 1;

    bool firstSpawn = true;

    // Start is called before the first frame update
    void Start()
    {
        arLen = thingsToActivate.Length;

        previewObjects = new GameObject[arLen];
        foreach (GameObject ob in thingsToActivate)
        {
            GameObject prev = Instantiate(previewPrefab, ob.transform.position, Quaternion.identity);
            previewObjects[arCount++] = prev;
            ob.SetActive(false);
        }
        arCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartPlatformSequence()
    {
        StartCoroutine(ActivatePlatform());
    }

    public void HideVisual()
    {
        transform.localScale = new Vector3(0, 0, 0);
    }

    private IEnumerator ActivatePlatform()
    {
        if (firstSpawn)
        {
            yield return new WaitForSeconds(.5f);
        } else
        {
            yield return new WaitForSeconds(spawnDelay);
        }

        GameObject obj = thingsToActivate[arCount];
        obj.SetActive(true);
        Instantiate(spawnEffectPrefab, obj.transform.position, Quaternion.identity);

        Destroy(previewObjects[arCount++]);
        if (arCount != arLen)
        {
            StartCoroutine(ActivatePlatform());
        }
    }
}
