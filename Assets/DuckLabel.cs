using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DuckLabel : MonoBehaviour
{
    private Duck duck;

    [SerializeField] private Image hungerBar;
    [SerializeField] private TextMeshProUGUI nameTag;

    [SerializeField] private float yOffset;

    public void SetDuck(Duck _duck)
    {
        duck = _duck;
        nameTag.text = duck.duckName;
    }

    void Update()
    {
        if (duck != null) 
        { 
            transform.position = Camera.main.WorldToScreenPoint(duck.labelAnchor.position);
            hungerBar.rectTransform.sizeDelta = new Vector2(50 * duck.satiety, 3);
        }
    }
}
