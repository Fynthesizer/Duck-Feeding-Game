using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Time Settings")] 
public class TimePeriodSettings : ScriptableObject
{
    public Material skyboxMaterial;
    public Color lightColour;
    public float lightIntensity;
    public Color fogColour;
}
