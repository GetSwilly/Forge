using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Identifier : MonoBehaviour {

    [SerializeField]
    new string name = "NULL";

     public string Name
    {
        get { return name; }
    }
}
