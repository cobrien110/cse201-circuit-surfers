using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBonus : MonoBehaviour
{
    public float speed = 2f;
    public RectTransform rt;
    // Start is called before the first frame update
    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rt.transform.position = new Vector3(rt.transform.position.x, rt.transform.position.y + (speed * Time.deltaTime), rt.transform.position.z);
    }
}
