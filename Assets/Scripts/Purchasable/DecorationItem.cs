using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decoration", menuName = "Purchasable/Decoration")]
public class DecorationItem : Purchasable
{
    public GameObject objectPrefab;
    public GameObject blueprintPrefab;
    public PlacementSurfaces placementSurfaces;

    public override void Buy()
    {
        Debug.Log($"Bought {name} for ${price}.");
        base.Buy();
    }

    [System.Flags]
    public enum PlacementSurfaces
    {
        Water = 1,
        Land = 2
    }
}
