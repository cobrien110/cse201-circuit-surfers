using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectedProjectile : MonoBehaviour
{
    [SerializeField] public float speed = 5f;
    [SerializeField] public float damage = 30f;
    [SerializeField] private int lifetime = 10;
    [SerializeField] private float hitRange = 1.25f;

    public LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CallDelete());
        transform.rotation = GameObject.FindGameObjectWithTag("MainCamera").transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit objectHit;
        if (Physics.Raycast(transform.position, fwd, out objectHit, hitRange + .5f, groundLayer))
        {
            if (objectHit.transform.gameObject.tag != "Player" && objectHit.transform.gameObject.tag != "Enemy")
            {
                Destroy(this.gameObject);
            }
        }

        if (Physics.Raycast(transform.position, fwd, out objectHit, hitRange))
        {
            Enemy enem = objectHit.transform.gameObject.GetComponent<Enemy>();
            if (enem != null)
            {
                enem.Damage(damage);
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
