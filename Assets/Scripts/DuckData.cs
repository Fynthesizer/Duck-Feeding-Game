using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[System.Serializable]
public class DuckData
{
    [Header("Attributes")]
    public string duckName;
    public Gender gender;
    public float speed = 1;
    public float reactionTime = 0.1f;
    public float awarenessRadius = 10;

    [Header("Status")]
    public string lastFedTime;

    public enum Gender
    {
        Male,
        Female
    }

    public DuckData(DuckDatabase database)
    {
        speed = Random.Range(4f, 6f);
        reactionTime = Random.Range(0f, 1f);
        awarenessRadius = Random.Range(7.5f, 15f);
        duckName = database.defaultNames[Random.Range(0, database.defaultNames.Count)];
        gender = Random.value > 0.5f ? Gender.Male : Gender.Female;
    }
}
