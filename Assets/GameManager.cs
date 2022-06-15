using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int duckCount = 10;
    [SerializeField] private GameObject[] duckPrefabs;

    private Bounds lakeBounds;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    void Start()
    {
        lakeBounds = GameObject.FindGameObjectWithTag("Lake").GetComponent<Collider>().bounds;
        SpawnDucks(duckCount);
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
        if (Physics.Raycast(ray, out hit, 10))
        {
            return hit.collider.gameObject.layer == 4;
        }
        else return false;
    }
}
