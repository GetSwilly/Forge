using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(MovementController))]

public class Block : BaseUtilityBehavior
{
    [Tooltip("Multiplier to apply to speed while behavior is active")]
    [SerializeField]
    [Range(0f, 3f)]
    float speedMultiplier = 1f;

    [Tooltip("Multiplier to apply to rotation while behavior is active")]
    [SerializeField]
    [Range(0f, 3f)]
    float rotationMultiplier = 1f;

    [Tooltip("Relative directive to apply block in")]
    [SerializeField]
    protected Vector3 blockDirection = new Vector3(0, 0, 1);

    [Tooltip("Angle of effect in which blocking covers")]
    [SerializeField]
    [Range(0f, 180f)]
    protected float blockAngle = 20f;

    [Tooltip("Damage resistance to be applied to blocking")]
    [SerializeField]
    int damageResistance = 0;


    Health m_Health;
    MovementController m_Movement;
    


    public override void Awake()
    {
        base.Awake();

        m_Health = GetComponent<Health>();
        m_Movement = GetComponent<MovementController>();
    }



    public override void StartBehavior()
    {
        IsActive = true;

        m_Health.DamageResistance += damageResistance;
        m_Movement.AddSpeedMultiplier(speedMultiplier);
        m_Movement.AddRotationMultiplier(rotationMultiplier);
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        m_Health.DamageResistance -= damageResistance;
        m_Movement.RemoveSpeedMultiplier(speedMultiplier);
        m_Movement.RemoveRotationMultiplier(rotationMultiplier);

        base.EndBehavior(shouldNotifyActor, shouldNotifyActor);
    }
    

    


    public override bool CanStartSubBehavior
    {
        get { return false; }
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

    public override float GetBehaviorScore()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return "Block";
    }
}
