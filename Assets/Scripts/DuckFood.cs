using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckFood : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float buoyancy = 20;
    [SerializeField] private float despawnTime = 60f;

    public bool inWater = false;

    private AudioSource splashSource;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private GameObject ripple;
    private Bobbing bobbing;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        splashSource = gameObject.GetComponent<AudioSource>();
        bobbing = gameObject.GetComponent<Bobbing>();
        StartCoroutine(DespawnTimer());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 4 && !inWater) //On collision with water
        {
            inWater = true;
            rb.isKinematic = true;
            transform.position = new Vector3(transform.position.x, 5, transform.position.z);
            bobbing.enabled = true;
            //Duck.UpdateSurroundings();
            splashSource.Play();
            particleSystem.Play();
            CreateRipple();
        }
    }

    private void OnDestroy()
    {
        //Duck.UpdateSurroundings();
        //GameManager.Instance.CheckGameEnded();
    }

    private void CreateRipple()
    {
        Vector3 ripplePosition = new Vector3(transform.position.x, 5.01f, transform.position.z);
        GameObject newRipple = Instantiate(ripple, ripplePosition, Quaternion.identity);
    }

    IEnumerator DespawnTimer()
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(gameObject);
    }

    /*
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
    */
}
