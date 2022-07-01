using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static UIManager UIManager;
    public static PlayerController player;
    public static SaveManager saveManager;

    public delegate void GameTick();
    public static event GameTick OnGameTick;

    public DuckDatabase duckInfoDatabase;
    public GameData gameData;
    public int currency;
    public int food;

    public float foodReplenishTimer = 0f;
    [Tooltip("How long it takes for food to be fully replenished, measured in seconds")]
    public float foodReplenishInterval = 60f;

    public List<Duck> ducks;

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
        //StartCoroutine(GameTickCoroutine());
        //if (saveManager.CheckForSaveData()) gameData = saveManager.LoadData(); //If a save file exists, load it
        //else gameData = new GameData(duckInfoDatabase, duckCount); //Otherwise, generate a new raft
    }

    private void Update()
    {
        UpdateFoodReplenishTimer(Time.deltaTime);
    }

    private void UpdateFoodReplenishTimer(float deltaTime)
    {
        if (food == ducks.Count) return;

        foodReplenishTimer += deltaTime * ducks.Count;

        if (foodReplenishTimer > foodReplenishInterval)
        {
            int replenishCount = Mathf.FloorToInt(foodReplenishTimer / foodReplenishInterval);
            foodReplenishTimer -= replenishCount * foodReplenishInterval;
            AddFood(replenishCount);
        }
    }

    private IEnumerator GameTickCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            OnGameTick();
        }
    }

    public void InitialiseGame(GameData data)
    {
        gameData = data;
        ducks = SpawnDucks();
        currency = data.currency;
        UIManager.UpdateCurrencyCount();
        food = data.foodCount;
        foodReplenishTimer = data.foodReplenishTimer;
        //ReplenishFood(DateTime.Parse(data.lastReplenishedFoodTime));
        UIManager.UpdateFoodCount();

        if (DateTime.TryParse(data.lastSaveTime, out DateTime lastSaveTime))
        {
            TimeSpan elapsedTime = DateTime.Now.Subtract(lastSaveTime);
            int elapsedSeconds = (int)elapsedTime.TotalSeconds;
            SimulateTime(elapsedSeconds);
        }
    }

    private void SimulateTime(int time)
    {
        print($"Simulating {time} seconds");
        foreach (Duck d in ducks) d.UpdateTicks(time);
        UpdateFoodReplenishTimer(time);
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
        UIManager.UpdateCurrencyCount();
    }

    public void AddFood(int amount)
    {
        food += amount;
        food = Mathf.Clamp(food, 0, ducks.Count);
        UIManager.UpdateFoodCount();
    }

    /*
    private void ReplenishFood(DateTime lastReplenishTime)
    {
        DateTime currentTime = DateTime.Now;
        TimeSpan timespan = currentTime.Subtract(lastReplenishTime);
        int amount = Mathf.FloorToInt((timespan.Minutes * gameData.raft.Count) / 60f);
        player.AddFood(amount);
        gameData.lastReplenishedFoodTime = DateTime.Now.ToString();
    }
    */

    private List<Duck> SpawnDucks()
    {
        List<Duck> duckList = new List<Duck>();

        for(int i = 0; i < gameData.raft.Count; i++)
        {
            Vector3 spawnPosition;
            //Find an appropriate position on lake
            while (true) { 
                spawnPosition = new Vector3(
                    Random.Range(lakeBounds.min.x, lakeBounds.max.x), 
                    lakeBounds.max.y, 
                    Random.Range(lakeBounds.min.z, lakeBounds.max.z));
                if (PositionIsOnLake(spawnPosition)) break;
            }

            GameObject newDuck = Instantiate(duckPrefab, spawnPosition, Quaternion.identity, duckGroup);
            newDuck.GetComponent<Duck>().LoadData(gameData.raft[i]);
            duckList.Add(newDuck.GetComponent<Duck>());
        }

        return duckList;
    }

    public bool PositionIsOnLake(Vector3 position)
    {
        Vector3 origin = new Vector3(position.x, 20, position.z);
        Ray ray = new Ray(origin, Vector3.down);
        if (Physics.SphereCast(ray, 2f, out RaycastHit hit))
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

    /*
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
    */
}