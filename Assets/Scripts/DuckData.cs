using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DuckData
{
    public string duckName;
    public Gender gender;
    public float speed = 1;
    public float reactionTime = 0.1f;
    public float awarenessRadius = 10;

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
