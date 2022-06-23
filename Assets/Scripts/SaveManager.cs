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


    public void SaveData(RaftData data)
    {
        string savePath = path;
        string json = JsonUtility.ToJson(data, true);

        using StreamWriter writer = new StreamWriter(savePath);
        writer.Write(json);
    }

    public bool CheckForSaveData()
    {
        return File.Exists(path);
    }

    public RaftData LoadData()
    {
        using StreamReader reader = new StreamReader(path);
        string json = reader.ReadToEnd();

        RaftData data = JsonUtility.FromJson<RaftData>(json);
        //Debug.Log(data.ToString());
        return data;
    }
}