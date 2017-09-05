using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityAttribute : AttributeEffect {

    public VisibilityAttribute(Transform _owner, AttributeHandler _handler, Transform _activator) : base(_owner, _handler, _activator)
    {
        Attribute = Attribute.Visibility;
        DecayRate = -10f;
        TransmissionRate = 30f;

        BurstRate = 0f;
        HealthDelta = 0f;

        SpreadThreshold = 0.5f;
    }
}
