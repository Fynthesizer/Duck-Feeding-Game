using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SaveManager : MonoBehaviour
{
    public GameData data;
    public DuckDatabase duckInfoDatabase;
    public int startingDuckCount = 10;

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

        if (CheckForSaveData()) data = LoadData();
        else data = NewGame(); //If no save data is found, create new game data

        GameManager.Instance.InitialiseGame(data);
    }

    private void SetPaths()
    {
        path = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveData.json";
        persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveData.json";

        activePath = usePersistentPath ? persistentPath : path;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) SaveData();
    }

    private IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveData();
        }
    }

    private GameData NewGame()
    {
        return new GameData(duckInfoDatabase, startingDuckCount);
    }

    public void UpdateGameData()
    {
        data.currency = GameManager.Instance.currency;
        data.foodCount = GameManager.Instance.food;
        data.foodReplenishTimer = GameManager.Instance.foodReplenishTimer;
        data.raft = GetRaftData();
        
        //data.lastReplenishedFoodTime = GameManager.Instance.gameData.lastReplenishedFoodTime;
    }

    public void SaveData()
    {
        print($"Game saved to {activePath}");

        UpdateGameData();
        data.lastSaveTime = System.DateTime.Now.ToString();

        string json = JsonUtility.ToJson(data, true);

        using StreamWriter writer = new StreamWriter(activePath);
        writer.Write(json);
    }

    public List<DuckData> GetRaftData()
    {
        List<DuckData> raftData = new List<DuckData>();
        foreach(Duck duck in FindObjectsOfType<Duck>())
        {
            raftData.Add(duck.Data);
        }
        return raftData;
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    public bool CheckForSaveData()
    {
        return File.Exists(activePath);
    }

    [ContextMenu("Clear Save Data")]
    private void ClearData()
    {
        print("Cleared save data");
        File.Delete(activePath);
    }

    private GameData LoadData()
    {
        print($"Game loaded from {activePath}");

        using StreamReader reader = new StreamReader(activePath);
        string json = reader.ReadToEnd();

        return JsonUtility.FromJson<GameData>(json);
    }
}
