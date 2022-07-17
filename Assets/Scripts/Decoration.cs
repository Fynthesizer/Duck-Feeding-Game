using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour
{
    public DecorationItem baseItem;

    public DecorationData Data { 
        get { return new DecorationData() { 
            DecorationID = baseItem.name, 
            Position = transform.position, 
            Rotation = transform.rotation }; 
        } 
    }
}
