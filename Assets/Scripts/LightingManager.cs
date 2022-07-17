using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance { get; private set; }

    [SerializeField] private bool automaticLighting;
    [SerializeField] TimePeriod timePeriod;
    [SerializeField] private TimePeriodSettings daySettings;
    [SerializeField] private TimePeriodSettings sunsetSettings;
    [SerializeField] private TimePeriodSettings nightSettings;

    public TimePeriodSettings CurrentSettings;

    [SerializeField] private ReflectionProbe reflectionProbe;
    
    public enum TimePeriod
    {
        Dawn,
        Day,
        Dusk,
        Night
    }

    private Dictionary<TimePeriod, TimePeriodSettings> timePeriodDictionary;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

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

        CurrentSettings = timePeriodDictionary[timePeriod];

        RenderSettings.skybox = CurrentSettings.skyboxMaterial;
        RenderSettings.customReflection = CurrentSettings.reflectionsCubemap;
        RenderSettings.sun.color = CurrentSettings.lightColour;
        RenderSettings.sun.intensity = CurrentSettings.lightIntensity;
        RenderSettings.sun.transform.eulerAngles = CurrentSettings.sunRotation;
        RenderSettings.fogColor = CurrentSettings.fogColour;

        DynamicGI.UpdateEnvironment();
        //reflectionProbe.RenderProbe();

        /*
        if (!Application.isPlaying) { 
            //Force the reflection probe re-render in the editor
            reflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
            reflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
        }
        */
        

        Lamp[] lamps = FindObjectsOfType<Lamp>();
        foreach(Lamp lamp in lamps)
        {
            lamp.Toggle(CurrentSettings.lampsEnabled);
        }

        GameObject[] clouds = GameObject.FindGameObjectsWithTag("Cloud");
        foreach(GameObject cloud in clouds)
        {
            cloud.GetComponent<MeshRenderer>().material = CurrentSettings.cloudsMaterial;
        }
    }
}
