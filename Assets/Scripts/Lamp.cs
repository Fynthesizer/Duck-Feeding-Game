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
        UpdateTimePeriod(LightingManager.Instance.CurrentSettings);
    }

    public void UpdateTimePeriod(TimePeriodSettings settings)
    {
        Toggle(settings.lampsEnabled);
    }

    public void Toggle(bool state)
    {
        gameObject.GetComponent<MeshRenderer>().materials = state ? onMaterials : offMaterials;
        halo.SetActive(state);
    }
}
