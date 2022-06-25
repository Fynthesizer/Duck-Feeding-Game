using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string path = "";
    private string persistentPath = "";

    void Start()
    {
        SetPaths();
    }

    private void SetPaths()
    {
        path = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveData.json";
        persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveData.json";
    }


    public void SaveData()
    {
        print("Game Saved");

        GameData data = new GameData();
        data.currency = GameManager.Instance.gameData.currency;
        data.raft = GetRaftData();

        string savePath = path;
        string json = JsonUtility.ToJson(data, true);

        using StreamWriter writer = new StreamWriter(savePath);
        writer.Write(json);
    }

    public List<DuckData> GetRaftData()
    {
        List<DuckData> duckData = new List<DuckData>();
        foreach(Duck duck in FindObjectsOfType<Duck>())
        {
            duckData.Add(duck.PackageData());
        }
        return duckData;
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    public bool CheckForSaveData()
    {
        return File.Exists(path);
    }

    public GameData LoadData()
    {
        print("Game Loaded");

        using StreamReader reader = new StreamReader(path);
        string json = reader.ReadToEnd();

        GameData data = JsonUtility.FromJson<GameData>(json);
        //Debug.Log(data.ToString());
        return data;
    }
}
