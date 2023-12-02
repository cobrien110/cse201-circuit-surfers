using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMeshCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Destroy(gameObject.GetComponent<MeshCollider>());
        }
        catch
        {
            //NOTHING
        } finally
        {
            gameObject.AddComponent<MeshCollider>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
