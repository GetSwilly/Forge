﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class MovementAffector : MonoBehaviour {

    [SerializeField]
    [Range(0f, 3f)]
    float speedMultipler = 1f;

    [SerializeField]
    [Range(0f, 3f)]
    float rotationMultipler = 1f;


    void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.isTrigger)
            return;

        MovementController _movement = coll.collider.GetComponent<MovementController>();

        _movement.AddSpeedMultiplier(this, speedMultipler);
        _movement.AddRotationMultiplier(this,rotationMultipler);
    }
    void OnCollisionExit(Collision coll)
    {
        if (coll.collider.isTrigger)
            return;


        MovementController _movement = coll.collider.GetComponent<MovementController>();

        _movement.RemoveSpeedMultiplier(this);
        _movement.RemoveRotationMultiplier(this);
    }
}
