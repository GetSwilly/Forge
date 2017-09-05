using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IceAttribute : AttributeEffect {

  
    [SerializeField]
    AnimationCurve speedSlowdownPercentageCurve = AnimationCurve.Linear(0f, 1f, 1f, .75f);
    float latestSpeedSlowdown = 1f;

    [SerializeField]
    AnimationCurve rotationSlowdownPercentageCurve = AnimationCurve.Linear(0f, 1f, 1f, .33f);
    float latestRotationSlowdown = 1f;
    
    MovementController m_Movement;


    public IceAttribute(Transform _owner, AttributeHandler _handler, Transform _activator) : base(_owner, _handler, _activator)
    {
        m_Movement = m_Owner.GetComponentInChildren<MovementController>();

       Attribute = Attribute.Ice;
        DecayRate = -10f;
        TransmissionRate = 30f;

        BurstRate = 2f;
        HealthDelta = 0f;

        SpreadThreshold = 0.5f;
    }


    public override void Update()
    {
        base.Update();

        DisableMovementEffects();
        latestSpeedSlowdown = SpeedEffect;
        latestRotationSlowdown = RotationEffect;
        EnableMovementEffects();
    }
    
    public override void Deactivate()
    {
        DisableMovementEffects();

        base.Deactivate();
    }





    public void EnableMovementEffects()
    {
        if (m_Movement != null)
        {
            m_Movement.AddSpeedMultiplier(latestSpeedSlowdown);
            m_Movement.AddRotationMultiplier(latestRotationSlowdown);
        }
    }
    public void DisableMovementEffects()
    {

        if (m_Movement != null)
        {
            m_Movement.RemoveSpeedMultiplier(latestSpeedSlowdown);
            m_Movement.RemoveRotationMultiplier(latestRotationSlowdown);
        }
    }



    public float SpeedEffect
    {
        get { return speedSlowdownPercentageCurve.Evaluate(GetPercentage()); }
    }
    public float RotationEffect
    {
        get { return rotationSlowdownPercentageCurve.Evaluate(GetPercentage()); }
    }
}
