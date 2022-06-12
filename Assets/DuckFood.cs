using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckFood : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float buoyancy = 20;

    List<DuckAI> interestedDucks;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 4) //On collision with water
        {
            //rb.isKinematic = true; //Freeze food's position
            DuckAI.UpdateSurroundings();
        }
    }

    private void OnDestroy()
    {
        DuckAI.UpdateSurroundings();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == 4)
        {
            rb.AddForce(new Vector3(0, buoyancy, 0));
            rb.drag = 20;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 4)
        {
            rb.drag = 2;
        }
    }
}
