using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private bool automaticLighting;
    [SerializeField] LightingMode lightingMode;
    [SerializeField] private TimePeriodSettings daySettings;
    [SerializeField] private TimePeriodSettings sunsetSettings;
    [SerializeField] private TimePeriodSettings nightSettings;

    [SerializeField] private ReflectionProbe reflectionProbe;
    
    public enum LightingMode
    {
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

    private LightingMode GetAutomaticLightingMode()
    {
        int time = System.DateTime.Now.Hour;
        if (time < 5 || time >= 20) return LightingMode.Night;
        else if (time < 8 || time >= 17) return LightingMode.Sunset;
        else return LightingMode.Day;
    }

    private void SetupLighting()
    {
        TimePeriodSettings timeSettings;

        if (automaticLighting) lightingMode = GetAutomaticLightingMode();

        switch (lightingMode)
        {
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
        reflectionProbe.RenderProbe();

        if (!Application.isPlaying) { 
            //Force the reflection probe re-render in the editor
            reflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
            reflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
        }

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
