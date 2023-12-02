using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] public float speed = 5f;
    [SerializeField] private float damage = 30f;
    [SerializeField] private int lifetime = 10;
    [SerializeField] public GameObject reflectedShotPrefab;
    [SerializeField] private float hitRange = 1.25f;
    public LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CallDelete());
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit objectHit;
        if (Physics.Raycast(transform.position, fwd, out objectHit, hitRange + .5f, groundLayer))
        {
            Destroy(this.gameObject);
        }

        if (Physics.Raycast(transform.position, fwd, out objectHit, hitRange))
        {
            PlayerController pc = objectHit.transform.gameObject.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Damage(damage);
                Destroy(this.gameObject);
            }
        }
    }

    private IEnumerator CallDelete()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(this.gameObject);
    }
}
