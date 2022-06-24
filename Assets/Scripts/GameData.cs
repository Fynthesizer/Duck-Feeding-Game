using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public List<DuckData> raft;
    public int currency;

    public GameData(DuckDatabase database, int duckCount)
    {
        raft = new List<DuckData>();
        currency = 0;

        for (int i = 0; i < duckCount; i++)
        {
            raft.Add(new DuckData(database));
        }
    }
}