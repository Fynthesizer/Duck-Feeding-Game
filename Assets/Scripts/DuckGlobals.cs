using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Duck Global Variables", menuName = "Duck/Global Variables")]
public class DuckGlobals : ScriptableObject
{
    public float pursuitSpeedMultiplier = 1.5f;
    public float turnSpeed = 2;
    public float minIdleTime = 0;
    public float maxIdleTime = 10;
    public float minQuackInterval = 2;
    public float maxQuackInterval = 30;
    public float eatTime = 1;
    public float wanderRadius = 5;
    public float satiationPeriod = 1;
    public float avoidDistance = 1f;
    public LayerMask wanderAvoidLayers;
    public LayerMask pursuitAvoidLayers;
}
