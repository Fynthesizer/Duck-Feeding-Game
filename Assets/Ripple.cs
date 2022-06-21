using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ripple : MonoBehaviour
{
    Material material;
    float time = 0f;

    void Start()
    {
        material = gameObject.GetComponent<MeshRenderer>().material;
        time = 0f;
        StartCoroutine(Disperse());
    }

    IEnumerator Disperse()
    {
        while(time < 1f)
        {
            time += Time.deltaTime;
            material.SetFloat("Time", time);
            yield return null;
        }
        Destroy(gameObject);
    }
}
