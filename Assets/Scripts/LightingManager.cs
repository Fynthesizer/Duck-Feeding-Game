using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    public LightingMode lightingMode;
    [SerializeField] private TimePeriodSettings daySettings;
    [SerializeField] private TimePeriodSettings sunsetSettings;
    [SerializeField] private TimePeriodSettings nightSettings;

    public enum LightingMode
    {
        TimeBased,
        Day,
        Sunset,
        Night
    }

    private void Start()
    {
        SetupLighting();
    }

    private void OnValidate()
    {
        SetupLighting();
    }

    private void SetupLighting()
    {
        TimePeriodSettings timeSettings;

        switch (lightingMode)
        {
            case LightingMode.TimeBased:
                int time = System.DateTime.Now.Hour;
                if (time < 6 || time >= 20) timeSettings = nightSettings;
                else if (time < 8 || time >= 18) timeSettings = sunsetSettings;
                else timeSettings = daySettings;
                break;
            case LightingMode.Day:
                timeSettings = daySettings;
                break;
            case LightingMode.Sunset:
                timeSettings = sunsetSettings;
                break;
            case LightingMode.Night:
                timeSettings = nightSettings;
                break;
            default:
                timeSettings = daySettings;
                break;
        }

        RenderSettings.skybox = timeSettings.skyboxMaterial;
        RenderSettings.sun.color = timeSettings.lightColour;
        RenderSettings.sun.intensity = timeSettings.lightIntensity;
        RenderSettings.sun.transform.eulerAngles = timeSettings.sunRotation;
        RenderSettings.fogColor = timeSettings.fogColour;
        //RenderSettings.ambientIntensity = timeSettings.lightIntensity;
        DynamicGI.UpdateEnvironment();

        Lamp[] lamps = FindObjectsOfType<Lamp>();
        foreach(Lamp lamp in lamps)
        {
            lamp.Toggle(timeSettings.lampsEnabled);
        }

        GameObject[] clouds = GameObject.FindGameObjectsWithTag("Cloud");
        foreach(GameObject cloud in clouds)
        {
            cloud.GetComponent<MeshRenderer>().material = timeSettings.cloudsMaterial;
        }


    }
}
