using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour
{
    public Material[] offMaterials;
    public Material[] onMaterials;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Toggle(bool state)
    {
        gameObject.GetComponent<MeshRenderer>().materials = state ? onMaterials : offMaterials;
    }
}
