using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public List<DuckData> raft;
    public List<DecorationData> decorations;
    public int currency;
    public int foodCount;
    public float foodReplenishTimer;
    public string lastSaveTime;

    public GameData(DuckDatabase database, int duckCount)
    {
        raft = new List<DuckData>();
        decorations = new List<DecorationData>();
        currency = 0;
        foodCount = duckCount;
        foodReplenishTimer = 0f;
        //lastReplenishedFoodTime = System.DateTime.Now.ToString();

        for (int i = 0; i < duckCount; i++)
        {
            raft.Add(new DuckData(database));
        }
    }

    public GameData()
    {

    }
}