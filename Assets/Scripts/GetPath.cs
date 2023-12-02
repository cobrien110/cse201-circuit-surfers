using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class GetPath : MonoBehaviour
{
    public PathCreator path;
    //public float transformOffset = 1f;
    // Start is called before the first frame update

    private MeshCollider mc;
    void Start()
    {
        PathCreator pathTest = GetComponentInParent<PathCreator>();
        //transform.position = new Vector3(transform.position.x, transform.position.y - transformOffset, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public PathCreator Path()
    {
        return path;
    }
}
