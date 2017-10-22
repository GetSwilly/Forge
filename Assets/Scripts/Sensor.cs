using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour {

    public delegate void CollisionEvent(Collision coll);
    public delegate void ColliderEvent(Collider other);

    public CollisionEvent CollisionEnter;
    public CollisionEvent CollisionStay;
    public CollisionEvent CollisionExit;

    public ColliderEvent TriggerEnter;
    public ColliderEvent TriggerStay;
    public ColliderEvent TriggerExit;

    void OnCollisionEnter(Collision collision)
    {
        if (CollisionEnter != null)
        {
            CollisionEnter(collision);
        }
    }
    void OnCollisionStay(Collision collision)
    {
        if (CollisionStay != null)
        {
            CollisionStay(collision);
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (CollisionExit != null)
        {
            CollisionExit(collision);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (TriggerEnter != null)
        {
            TriggerEnter(other);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (TriggerStay != null)
        {
            TriggerStay(other);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (TriggerExit != null)
        {
            TriggerExit(other);
        }
    }
}
