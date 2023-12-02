using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PathCollision : MonoBehaviour
{

    public PathCreator pathCreator;
    public float speed = 5;
    float distanceTravelled;
    bool onRail = false;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        MoveOnRail();
        if (Input.GetKey(KeyCode.Space))
        {
            onRail = false;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Road")
        {
            onRail = true;
        }
        Vector3 collisionPoint = other.ClosestPoint(transform.position);
    }

    private void MoveOnRail()
    {
        if (onRail)
        {
            distanceTravelled -= speed * Time.deltaTime;
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);
        }
    }
}
