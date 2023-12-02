using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] public float speed = 25f;
    [SerializeField] private int lifetime = 10;
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
            if (objectHit.transform.gameObject.tag != "Player")
            {
                Destroy(this.gameObject);
            }
        }
    }
    private IEnumerator CallDelete()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(this.gameObject);
    }

    public void SetRotation(Vector3 v)
    {
        transform.LookAt(v);
    }
}
