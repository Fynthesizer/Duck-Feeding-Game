using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour
{
    [SerializeField] private GameObject halo;

    public Material[] offMaterials;
    public Material[] onMaterials;
    void Start()
    {
        Toggle(LightingManager.Instance.CurrentSettings);
    }

    public void UpdateTimePeriod(TimePeriodSettings settings)
    {
        Toggle(settings.lampsEnabled);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Toggle(bool state)
    {
        gameObject.GetComponent<MeshRenderer>().materials = state ? onMaterials : offMaterials;
        halo.SetActive(state);
    }
}
