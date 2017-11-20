using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Identifier : MonoBehaviour,IIdentifier {

    [SerializeField]
    string myName;

    public string Name
    {
        get { return myName; }
    }
}
