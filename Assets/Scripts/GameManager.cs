using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Pathfinding;

using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static UIManager UIManager;
    public static PlayerController player;
    public static SaveManager saveManager;

    public InputActions Input;

    private Camera activeCamera;
    public GameObject editController;
    public Camera playerCamera;

    public delegate void GameTick();
    public static event GameTick OnGameTick;

    public GameState state;

    public static event Action<GameState> OnGameStateChanged;

    public StoreCatalog decorationDatabase;
    [SerializeField] private Transform decorationGroup;

    public DuckDatabase duckInfoDatabase;
    public GameData gameData;
    public int currency;
    public int food;

    [SerializeField] private AstarPath astar;

    public int Currency { get { return currency; } set { currency = Mathf.Max(value, 0); UIManager.UpdateCurrencyCount(); } }
    public int Food { get { return food; } set { food = Mathf.Clamp(value, 0, ducks.Count); UIManager.UpdateFoodCount(); } }

    public float foodReplenishTimer = 0f;
    [Tooltip("How long it takes for food to be fully replenished, measured in seconds")]
    public float foodReplenishInterval = 60f;

    private DateTime pauseTime;

    public List<Duck> ducks;
    [SerializeField] private GameObject duckPrefab;
    [SerializeField] private Transform duckGroup;

    private Bounds lakeBounds;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);

        Application.targetFrameRate = 60;
        Input = new InputActions();
        Input.Player.Touch.Enable();

        UIManager = gameObject.GetComponent<UIManager>();
        saveManager = gameObject.GetComponent<SaveManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        lakeBounds = GameObject.FindGameObjectWithTag("Lake").GetComponent<Collider>().bounds;
    }

    void Start()
    {
        SetGameState(GameState.Feeding);
    }

    private void OnApplicationFocus(bool focus)
    {
        
    }

    public void SetGameState(GameState newState)
    {
        if (state == newState) return;

        state = newState;

        editController.SetActive(false);
        player.gameObject.SetActive(false);

        switch (state)
        {
            case GameState.Decorating:
                editController.SetActive(true);
                //SetActiveCamera(editController.GetComponent<Camera>());
                break;
            case GameState.Feeding:
                player.gameObject.SetActive(true);
                //SetActiveCamera(playerCamera);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }

    public void SetGameStateTest(int newState)
    {
        SetGameState((GameState)newState);
    }

    private void SetActiveCamera(Camera newCamera)
    {
        if (activeCamera == newCamera) return;

        if (activeCamera != null)
        {
            activeCamera.enabled = false;
            activeCamera.GetComponent<GyroscopeControls>().enabled = false;
        }
        activeCamera = newCamera;
        activeCamera.enabled = true;
        activeCamera.GetComponent<GyroscopeControls>().enabled = true;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) pauseTime = DateTime.Now;
        else
        {
            int elapsedTime = (int)DateTime.Now.Subtract(pauseTime).TotalSeconds;
            if (elapsedTime > 0) SimulateTime(elapsedTime);
        }
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
            Food += replenishCount;
            //AddFood(replenishCount);
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
        SpawnDecorations();
        currency = data.currency;
        UIManager.UpdateCurrencyCount();
        food = data.foodCount;
        foodReplenishTimer = data.foodReplenishTimer;
        //ReplenishFood(DateTime.Parse(data.lastReplenishedFoodTime));
        UIManager.UpdateFoodCount();
        UIManager.CreateDuckLabels(ducks);
        astar.Scan();

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

    /*
    public void AddCurrency(int amount)
    {
        currency += amount;
        UIManager.UpdateCurrencyCount();
    }

    public bool SubtractCurrency(int amount)
    {
        if (currency > amount)
        {
            currency -= amount;
            UIManager.UpdateCurrencyCount();
            return true;
        }
        else return false;
    }

    public void AddFood(int amount)
    {
        food += amount;
        food = Mathf.Clamp(food, 0, ducks.Count);
        UIManager.UpdateFoodCount();
    }
    /*

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

    private void SpawnDecorations()
    {
        for(int i = 0; i < gameData.decorations.Count; i++)
        {
            DecorationData decorationData = gameData.decorations[i];
            DecorationItem decoration = decorationDatabase.Decorations.Find(x => x.name.Equals(decorationData.DecorationID));

            GameObject newDecoration = Instantiate(decoration.objectPrefab, decorationData.Position, decorationData.Rotation, decorationGroup);
            newDecoration.transform.localScale = decorationData.Scale;
        }
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
}

public enum GameState
{
    Feeding,
    Decorating
}