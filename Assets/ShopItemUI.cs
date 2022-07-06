using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Purchasable _item;

    [SerializeField] private Image _itemThumbnail;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _itemPrice;

    public void SetItem(Purchasable item)
    {
        _item = item;
        gameObject.name = item.name;

        _itemThumbnail.sprite = item.sprite;
        _itemName.text = item.name;
        _itemPrice.text = $"${item.price}";
    }

    public void OnClick()
    {
        print("Clicked");
        _item.Buy();
    }
}
