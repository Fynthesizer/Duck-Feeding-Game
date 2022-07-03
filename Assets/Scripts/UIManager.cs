using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI foodCount;
    [SerializeField] private TextMeshProUGUI currencyCount;
    [SerializeField] private GameObject gameEndScreen;

    [SerializeField] private GameObject duckLabelGroup;
    [SerializeField] private GameObject duckLabelPrefab;

    private GameObject[] duckLabels;
    void Start()
    {
        UpdateFoodCount();
    }

    void Update()
    {
        
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

        for(int i = 0; i < ducks.Count; i++)
        {
            duckLabels[i] = Instantiate(duckLabelPrefab, duckLabelGroup.transform);
            duckLabels[i].GetComponent<DuckLabel>().SetDuck(ducks[i]);
        }
    }
}
