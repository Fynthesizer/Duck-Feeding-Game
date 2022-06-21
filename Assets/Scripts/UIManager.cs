using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI foodCount;
    void Start()
    {
        UpdateFoodCount();
    }

    void Update()
    {
        
    }


    public void UpdateFoodCount()
    {
        foodCount.text = $"Food: {PlayerController.availableFood}";
    }
}
