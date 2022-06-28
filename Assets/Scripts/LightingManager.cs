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
        if (lightingMode == LightingMode.TimeBased)
        {
            int time = System.DateTime.Now.Hour;
            if (time < 6 || time >= 20) timeSettings = nightSettings;
            else if (time < 8 || time >= 18) timeSettings = sunsetSettings;
            else timeSettings = daySettings;
        }
        else if (lightingMode == LightingMode.Day) timeSettings = daySettings;
        else if (lightingMode == LightingMode.Sunset) timeSettings = sunsetSettings;
        else if (lightingMode == LightingMode.Night) timeSettings = nightSettings;
        else timeSettings = daySettings;

        RenderSettings.skybox = timeSettings.skyboxMaterial;
        RenderSettings.sun.color = timeSettings.lightColour;
        RenderSettings.sun.intensity = timeSettings.lightIntensity;
        RenderSettings.fogColor = timeSettings.fogColour;
        //RenderSettings.ambientIntensity = timeSettings.lightIntensity;
        DynamicGI.UpdateEnvironment();
    }
}
