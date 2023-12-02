using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Follower : MonoBehaviour
{
    public PathCreator pathCreator;
    public float speed = 5;
    public float offSet = 1f;
    float distanceTravelled;

    void Update()
    {
        distanceTravelled -= speed * Time.deltaTime;
        transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled);
        transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);

        transform.position -= new Vector3 (0f, offSet, 0f);
    }
}
