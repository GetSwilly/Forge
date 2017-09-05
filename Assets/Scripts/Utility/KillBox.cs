using UnityEngine;
using System.Collections;

public class KillBox : MonoBehaviour {

    void OnTriggerEnter(Collider coll)
    {
        Destroy(coll.gameObject);
    }
}
