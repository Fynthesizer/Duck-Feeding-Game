using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bobbing : MonoBehaviour
{
    [SerializeField] private float bobSpeed;
    [SerializeField] private float bobAmount;
    [SerializeField] private float bobOffset;

    void Update()
    {
        float timeOffset = transform.position.x + transform.position.z;
        float yPos = (Mathf.Sin(Time.time * bobSpeed + timeOffset) * bobAmount) + bobOffset;
        transform.localPosition = new Vector3(0, yPos, 0);
    }
}
