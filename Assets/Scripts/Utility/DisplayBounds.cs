using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayBounds : MonoBehaviour {

    //[SerializeField]
    //bool checkTriggers = true;

    void OnDrawGizmos()
    {
        Bounds _bounds = Utilities.CalculateObjectBounds(gameObject);

        Gizmos.DrawWireCube(_bounds.center, _bounds.size);
    }
}
