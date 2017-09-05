using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaterAttribute : AttributeEffect {

    public WaterAttribute(Transform _owner, AttributeHandler _handler, Transform _activator) : base(_owner, _handler, _activator)
    {
        Attribute = Attribute.Water;
        DecayRate = -10f;
        TransmissionRate = 30f;

        BurstRate = 2f;
        HealthDelta = 0f;

        SpreadThreshold = 0.5f;
    }

}
