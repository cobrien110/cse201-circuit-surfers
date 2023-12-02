using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTest : MonoBehaviour
{
    // Start is called before the first frame update
    public float hp = 30f;

    public void Damage(float amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
