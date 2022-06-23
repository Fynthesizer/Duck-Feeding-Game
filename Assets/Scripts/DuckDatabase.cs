using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Duck Info Database", order = 1)]
public class DuckDatabase : ScriptableObject
{
    public List<string> defaultNames;
}
