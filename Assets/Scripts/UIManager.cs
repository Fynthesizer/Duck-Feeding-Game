using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public bool showDuckLabels = false;

    [SerializeField] private TextMeshProUGUI foodCount;
    [SerializeField] private TextMeshProUGUI currencyCount;
    [SerializeField] private GameObject gameEndScreen;

    
    [SerializeField] private GameObject duckLabelPrefab;

    [SerializeField] private GameObject shopInterface;
    [SerializeField] private GameObject editInterface;
    [SerializeField] private GameObject feedInterface;

    private GameObject[] duckLabels;
    void Start()
    {
        UpdateFoodCount();
    }

    void Update()
    {
        
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    public void UpdateFoodCount()
    {
        foodCount.text = $"Food: {GameManager.Instance.food}";
    }

    public void UpdateCurrencyCount()
    {
        currencyCount.text = $"DuckBux: ${GameManager.Instance.currency}";
    }

    public void GameEnd(float score)
    {
        string scoreText = $"Score: {score.ToString("0.0")} / 5";
        gameEndScreen.SetActive(true);
        gameEndScreen.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = scoreText;
    }

    public void CreateDuckLabels(List<Duck> ducks)
    {
        duckLabels = new GameObject[ducks.Count];

        Transform labelGroup = feedInterface.transform.GetChild(0);

        for(int i = 0; i < ducks.Count; i++)
        {
            duckLabels[i] = Instantiate(duckLabelPrefab, labelGroup);
            duckLabels[i].GetComponent<DuckLabel>().SetDuck(ducks[i]);
        }

        ToggleDuckLabels(showDuckLabels);
    }

    private void OnGameStateChanged(GameState newState)
    {
        feedInterface.SetActive(false);
        editInterface.SetActive(false);

        switch (newState)
        {
            case GameState.Decorating:
                editInterface.SetActive(true);
                break;
            case GameState.Feeding:
                feedInterface.SetActive(true);
                break;
        }

        if (newState == GameState.Feeding) ToggleDuckLabels(true);
        else ToggleDuckLabels(false);
    }

    public void ToggleDuckLabels(bool state)
    {
        feedInterface.SetActive(state);
    }
}
