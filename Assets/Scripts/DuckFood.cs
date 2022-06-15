using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckFood : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float buoyancy = 20;

    public bool inWater = false;

    List<Duck> interestedDucks;

    private AudioSource splashSource;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        splashSource = gameObject.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 4 && !inWater) //On collision with water
        {
            //rb.isKinematic = true; //Freeze food's position
            inWater = true;
            Duck.UpdateSurroundings();
            splashSource.Play();
        }

        else if (other.gameObject.layer == 6) //On collision with terrain
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Duck.UpdateSurroundings();
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
