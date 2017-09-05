using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class GrappleHook : MonoBehaviour {


    bool canHook = true;


    Transform hookedTransform;
 

    public delegate void AlertHook();
    public AlertHook OnHook;

  

    public bool CanHook
    {
        get { return canHook; }
        set { canHook = value; }
    }
    public Transform HookedTransform
    {
        get { return hookedTransform; }
    }



    void OnCollisionEnter(Collision coll)
    {
        if (!CanHook || coll.collider.isTrigger)
            return;


        hookedTransform = coll.transform;

        if (OnHook != null)
            OnHook();
    }
}
