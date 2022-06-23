using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static UIManager UIManager;
    public static PlayerController player;
    public static SaveManager saveManager;

    public DuckDatabase duckInfoDatabase;
    public RaftData raftData;

    public int duckCount = 10;
    [SerializeField] private GameObject[] duckPrefabs;

    [SerializeField] private Transform duckGroup;

    [SerializeField] private Material daySkybox;
    [SerializeField] private Material sunsetSkybox;
    [SerializeField] private Material nightSkybox;

    private Bounds lakeBounds;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    void Start()
    {
        UIManager = gameObject.GetComponent<UIManager>();
        saveManager = gameObject.GetComponent<SaveManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        lakeBounds = GameObject.FindGameObjectWithTag("Lake").GetComponent<Collider>().bounds;

        SetSkybox();

        if (saveManager.CheckForSaveData()) raftData = saveManager.LoadData(); //If a save file exists, load it
        else raftData = new RaftData(duckInfoDatabase, duckCount); //Otherwise, generate a new raft

        SpawnDucks();
    }

    private void SetSkybox()
    {
        int time = System.DateTime.Now.Hour;
        if (time < 6 || time > 20) RenderSettings.skybox = nightSkybox;
        else if (time < 8 || time > 18) RenderSettings.skybox = sunsetSkybox;
        else RenderSettings.skybox = daySkybox;
    }

    private void GenerateRaft(int count)
    {
        raftData = new RaftData(duckInfoDatabase, count);
    }

    private void OnApplicationQuit()
    {
        saveManager.SaveData(raftData);
    }

    void SpawnDucks()
    {
        for(int i = 0; i < raftData.raft.Count; i++)
        {
            Vector3 spawnPosition;
            //Find an appropriate position on lake
            while (true) { 
                spawnPosition = new Vector3(
                    Random.Range(lakeBounds.min.x, lakeBounds.max.x), 
                    lakeBounds.max.y, 
                    Random.Range(lakeBounds.min.z, lakeBounds.max.z));
                if (!PositionIsOnLake(spawnPosition)) continue;
                else break;
            }

            GameObject duckPrefab = duckPrefabs[Random.Range(0, duckPrefabs.Length)];
            GameObject newDuck = Instantiate(duckPrefab, spawnPosition, Quaternion.identity, duckGroup);
            newDuck.GetComponent<Duck>().SetData(raftData.raft[i]);
        }
    }

    public bool PositionIsOnLake(Vector3 position)
    {
        RaycastHit hit;
        Vector3 origin = new Vector3(position.x, 10, position.z);
        Ray ray = new Ray(origin, Vector3.down);
        if (Physics.SphereCast(ray, 2f, out hit, 10))
        {
            return hit.collider.gameObject.layer == 4;
        }
        else return false;
    }

    public void CheckGameEnded()
    {
        if (PlayerController.availableFood > 0) return;
        else if (GameObject.FindGameObjectsWithTag("DuckFood").Length > 0) return;
        else GameEnd();
    }

    public void GameEnd()
    {
        GameObject[] ducks = GameObject.FindGameObjectsWithTag("Duck");
        int fedCount = 0;
        foreach (GameObject duckObject in ducks)
        {
            Duck duck = duckObject.GetComponent<Duck>();
            if (duck.foodConsumed > 0) fedCount += 1;
        }
        float fedPercent = (float)fedCount / (float)ducks.Length;
        print($"Fed {fedCount} of {ducks.Length} ducks");
        float score = fedPercent * 5f;
        print($"Score: {score.ToString("0.0")} / 5");

        UIManager.GameEnd(score);
    }
}
