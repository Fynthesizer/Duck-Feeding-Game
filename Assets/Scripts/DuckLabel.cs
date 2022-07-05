using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DuckLabel : MonoBehaviour
{
    private Duck duck;

    [SerializeField] private float hungerBarLerpSpeed = 10f;

    [SerializeField] private Image hungerBar;
    [SerializeField] private TextMeshProUGUI nameTag;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void SetDuck(Duck _duck)
    {
        duck = _duck;
        nameTag.text = duck.duckName;
        hungerBar.rectTransform.sizeDelta = new Vector2(duck.satiety * 50f, 3);
    }

    void LateUpdate()
    {
        if (duck != null) 
        {
            float barTargetWidth = duck.satiety * 50f;
            float barWidth = Mathf.Lerp(hungerBar.rectTransform.sizeDelta.x, barTargetWidth, Time.deltaTime * hungerBarLerpSpeed);
            transform.position = cam.WorldToScreenPoint(duck.labelAnchor.position);
            hungerBar.rectTransform.sizeDelta = new Vector2(barWidth, 3);
        }
    }
}
