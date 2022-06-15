using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bobbing : MonoBehaviour
{
    [SerializeField] private float bobSpeed;
    [SerializeField] private float bobAmount;
    [SerializeField] private float bobOffset;
    [SerializeField] private float waterHeight = 5f;

    private Vector3 basePos;

    private void Start()
    {
        basePos = transform.localPosition;
    }
    void Update()
    {
        float timeOffset = transform.position.x + transform.position.z;
        float yOffset = (Mathf.Sin(Time.time * bobSpeed + timeOffset) * bobAmount) + bobOffset;
        transform.localPosition = new Vector3(basePos.x, basePos.y + yOffset, basePos.z);
    }
}
