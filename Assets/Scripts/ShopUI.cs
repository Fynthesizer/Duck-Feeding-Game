using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private StoreCatalog catalog;
    [SerializeField] private GameObject itemUIPrefab;

    [SerializeField] private Transform catalogUI;

    private List<GameObject> shopItems = new List<GameObject>();

    void Start()
    {
        PopulateStore();
    }

    private void PopulateStore()
    {
        foreach(Decoration d in catalog.Decorations)
        {
            GameObject newElement = Instantiate(itemUIPrefab, catalogUI);
            newElement.GetComponent<ShopItemUI>().SetItem(d);
            shopItems.Add(newElement);
        }
    }
}
