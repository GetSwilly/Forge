using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetector : MonoBehaviour {

    public delegate void AlertEvent(Collider coll);
    public AlertEvent OnHit;


    void OnTriggerEnter(Collider coll)
    {
        if(OnHit != null)
        {
            OnHit.Invoke(coll);
        }
    }
}
