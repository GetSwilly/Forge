using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FireAttribute : AttributeEffect
{
    public FireAttribute(Transform _owner, AttributeHandler _handler, Transform _activator) : base(_owner, _handler, _activator)
    {
       Attribute = Attribute.Fire;
        DecayRate = -6f;
        TransmissionRate = 18f;

        BurstRate = 2f;
        HealthDelta = 2f;

        SpreadThreshold = 0.5f;
    }
}
