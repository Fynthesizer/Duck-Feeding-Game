using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Purchasable : ScriptableObject
{
    public string name;
    public int price;
    public Sprite sprite;
    public virtual void Buy()
    {
        GameManager.Instance.SubtractCurrency(price);
    }
}