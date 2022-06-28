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
    public GameData gameData;
    public int currency;

    public int duckCount = 10;
    [SerializeField] private GameObject duckPrefab;

    [SerializeField] private Transform duckGroup;

    private Bounds lakeBounds;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);

        UIManager = gameObject.GetComponent<UIManager>();
        saveManager = gameObject.GetComponent<SaveManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        lakeBounds = GameObject.FindGameObjectWithTag("Lake").GetComponent<Collider>().bounds;
    }

    void Start()
    {
        if (saveManager.CheckForSaveData()) gameData = saveManager.LoadData(); //If a save file exists, load it
        else gameData = new GameData(duckInfoDatabase, duckCount); //Otherwise, generate a new raft

        OnLoad();
    }

    public void OnLoad()
    {
        SpawnDucks();
        currency = gameData.currency;
        UIManager.UpdateCurrencyCount();
        player.availableFood = gameData.foodCount;
        ReplenishFood();
        UIManager.UpdateFoodCount();
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
        UIManager.UpdateCurrencyCount();
    }

    private void ReplenishFood()
    {
        System.DateTime lastReplenishTime = System.DateTime.Parse(gameData.lastReplenishedFoodTime);
        System.DateTime currentTime = System.DateTime.Now;
        System.TimeSpan timespan = currentTime.Subtract(lastReplenishTime);
        int amount = Mathf.FloorToInt((timespan.Minutes * gameData.raft.Count) / 60f);
        player.AddFood(amount);
        gameData.lastReplenishedFoodTime = System.DateTime.Now.ToString();
    }

    void SpawnDucks()
    {
        for(int i = 0; i < gameData.raft.Count; i++)
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

            GameObject newDuck = Instantiate(duckPrefab, spawnPosition, Quaternion.identity, duckGroup);
            newDuck.GetComponent<Duck>().LoadData(gameData.raft[i]);
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

    /*
    public void CheckGameEnded()
    {
        if (PlayerController.availableFood > 0) return;
        else if (GameObject.FindGameObjectsWithTag("DuckFood").Length > 0) return;
        else GameEnd();
    }
    */

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