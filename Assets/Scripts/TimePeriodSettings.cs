using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Time Settings")] 
public class TimePeriodSettings : ScriptableObject
{
    public Material skyboxMaterial;
    public Cubemap reflectionsCubemap;
    public Color lightColour;
    public float lightIntensity;
    public Color fogColour;

    public bool lampsEnabled;

    public Material cloudsMaterial;
    public Vector3 sunRotation;
}
