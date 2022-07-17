using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Store", menuName = "Store")]
public class StoreCatalog : ScriptableObject
{
    public List<DecorationItem> Decorations;
}
