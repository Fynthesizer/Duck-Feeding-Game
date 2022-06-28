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
    void Start()
    {
        UpdateFoodCount();
    }

    void Update()
    {
        
    }


    public void UpdateFoodCount()
    {
        foodCount.text = $"Food: {GameManager.player.availableFood}";
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
}
