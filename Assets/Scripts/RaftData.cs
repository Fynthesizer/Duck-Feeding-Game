using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaftData
{
    public List<DuckData> raft;

    public RaftData(DuckDatabase database, int count)
    {
        raft = new List<DuckData>();

        for (int i = 0; i < count; i++)
        {
            raft.Add(new DuckData(database));
        }
    }
}