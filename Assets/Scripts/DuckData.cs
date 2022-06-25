using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

[System.Serializable]
public class DuckData
{
    [Header("Attributes")]
    public string duckName;
    public Duck.Gender gender;
    //public DuckBreed breed;
    public string breed;
    public float speed;
    public float weight;
    public float reactionTime;
    public float awarenessRadius;

    [Header("Status")]
    public string lastFedTime;

    public DuckData(DuckDatabase database)
    {
        speed = Random.Range(4f, 6f);
        reactionTime = Random.Range(0f, 1f);
        awarenessRadius = Random.Range(7.5f, 15f);
        weight = Random.Range(0.8f, 1.6f);
        gender = Random.value > 0.5f ? Duck.Gender.Male : Duck.Gender.Female;

        //Filter all possible names to those matching the ducks gender and unisex names
        List<DuckName> possibleNames = database.possibleNames.Where(name => name.gender == gender || name.gender == Duck.Gender.Unisex).ToList();
        duckName = possibleNames[Random.Range(0, possibleNames.Count)].name;
        breed = database.breeds[Random.Range(0, database.breeds.Count)].breedName;
    }

    public DuckData()
    {

    }
}
