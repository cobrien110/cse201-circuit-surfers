using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageField : MonoBehaviour
{
    public float damage = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.tag.Equals("Player"))
        {
            PlayerController pc = other.transform.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Damage(damage);
            }
            PlayerControllerNetwork netPc = other.transform.GetComponent<PlayerControllerNetwork>();
            if (netPc != null)
            {
                netPc.Damage(damage);
            }
        }
    }
}
