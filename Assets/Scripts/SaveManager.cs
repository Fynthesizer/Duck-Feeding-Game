using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string path = "";
    private string persistentPath = "";

    [SerializeField] private bool usePersistentPath = true;

    private string activePath;

    [SerializeField] float autoSaveInterval;
    [SerializeField] bool autoSaveEnabled;

    void Start()
    {
        SetPaths();
        if (autoSaveEnabled) StartCoroutine(AutoSave());
    }

    private void SetPaths()
    {
        path = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveData.json";
        persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveData.json";

        activePath = usePersistentPath ? persistentPath : path;
    }

    private IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveData();
        }
    }

    public void SaveData()
    {
        print($"Game saved to {activePath}");

        GameData data = new GameData();
        data.currency = GameManager.Instance.currency;
        data.foodCount = GameManager.player.availableFood;
        data.raft = GetRaftData();
        data.lastReplenishedFoodTime = GameManager.Instance.gameData.lastReplenishedFoodTime;

        string json = JsonUtility.ToJson(data, true);

        using StreamWriter writer = new StreamWriter(activePath);
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
        return File.Exists(activePath);
    }

    public GameData LoadData()
    {
        print($"Game loaded from {activePath}");

        using StreamReader reader = new StreamReader(activePath);
        string json = reader.ReadToEnd();

        GameData data = JsonUtility.FromJson<GameData>(json);
        //Debug.Log(data.ToString());
        return data;
    }
}
