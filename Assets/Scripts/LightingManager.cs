using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private bool automaticLighting;
    [SerializeField] TimePeriod timePeriod;
    [SerializeField] private TimePeriodSettings daySettings;
    [SerializeField] private TimePeriodSettings sunsetSettings;
    [SerializeField] private TimePeriodSettings nightSettings;

    [SerializeField] private ReflectionProbe reflectionProbe;
    
    public enum TimePeriod
    {
        Dawn,
        Day,
        Dusk,
        Night
    }

    private Dictionary<TimePeriod, TimePeriodSettings> timePeriodDictionary;

    private void Start()
    {
        SetupLighting();
    }

    private void OnValidate()
    {
        SetupLighting();
    }

    private TimePeriod GetAutomaticTimePeriod()
    {
        int time = System.DateTime.Now.Hour;
        if (time < 5 || time >= 20) return TimePeriod.Night;
        else if (time < 8) return TimePeriod.Dawn;
        else if (time >= 17) return TimePeriod.Dusk;
        else return TimePeriod.Day;
    }

    private void SetupLighting()
    {
        if (automaticLighting) timePeriod = GetAutomaticTimePeriod();

        timePeriodDictionary = new Dictionary<TimePeriod, TimePeriodSettings>()
        {
            {TimePeriod.Dawn, sunsetSettings },
            {TimePeriod.Day, daySettings },
            {TimePeriod.Dusk, sunsetSettings },
            {TimePeriod.Night, nightSettings },
        };

        TimePeriodSettings timeSettings = timePeriodDictionary[timePeriod];

        RenderSettings.skybox = timeSettings.skyboxMaterial;
        RenderSettings.sun.color = timeSettings.lightColour;
        RenderSettings.sun.intensity = timeSettings.lightIntensity;
        RenderSettings.sun.transform.eulerAngles = timeSettings.sunRotation;
        RenderSettings.fogColor = timeSettings.fogColour;

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
