using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public List<DuckData> raft;
    public int currency;
    public int foodCount;
    public string lastReplenishedFoodTime;

    public GameData(DuckDatabase database, int duckCount)
    {
        raft = new List<DuckData>();
        currency = 0;
        foodCount = duckCount;
        lastReplenishedFoodTime = System.DateTime.Now.ToString();

        for (int i = 0; i < duckCount; i++)
        {
            raft.Add(new DuckData(database));
        }
    }

    public GameData()
    {

    }
}