using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static UIManager UIManager;

    public int duckCount = 10;
    [SerializeField] private GameObject[] duckPrefabs;

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
        lakeBounds = GameObject.FindGameObjectWithTag("Lake").GetComponent<Collider>().bounds;
        SpawnDucks(duckCount);
        SetSkybox();
        
    }

    private void SetSkybox()
    {
        int time = System.DateTime.Now.Hour;
        if (time < 6 || time > 20) RenderSettings.skybox = nightSkybox;
        else if (time < 8 || time > 18) RenderSettings.skybox = sunsetSkybox;
        else RenderSettings.skybox = daySkybox;
    }

    void Update()
    {
        
    }

    void SpawnDucks(int count)
    {
        for(int i = 0; i < count; i++)
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
            Instantiate(duckPrefab, spawnPosition, Quaternion.identity);
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
}
