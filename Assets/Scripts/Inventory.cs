using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Inventory
{
    public List<InventorySlot> Slots;

    public Inventory()
    {
        Slots = new List<InventorySlot>();
    }

    public void AddItem(DecorationItem item, int quantity = 1)
    {
        InventorySlot slot = Slots.Find(i => i.Item == item);

        if (slot == null) Slots.Add(new InventorySlot { Item = item, Quantity = quantity });
        else slot.Quantity += quantity;
    }
}

[System.Serializable]
public class InventorySlot
{
    public int Quantity;
    public DecorationItem Item;
}