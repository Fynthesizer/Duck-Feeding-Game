using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decoration", menuName = "Purchasable/Decoration")]
public class Decoration : Purchasable
{
    public GameObject objectPrefab;

    public override void Buy()
    {
        Debug.Log($"Bought {name} for ${price}.");
        base.Buy();
    }
}
