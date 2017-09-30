﻿using UnityEngine;
using System.Collections;
using System;

public class Chase : BaseUtilityBehavior
{
    enum Type
    {
        LastKnownPosition,
        Transform
    }

    [SerializeField]
    Type m_Type;


    [Tooltip("Maximum amount of time an object can be out of sight before behavior ends. -1 means infinity.")]
    [SerializeField]
    float maxTimeOutOfSight = 2f;

    [Tooltip("Multiplier to be applied to speed while behavior is active")]
    [SerializeField]
    [Range(0f, 3f)]
    float chaseMovementSpeedup = 1f;

    [Tooltip("Multiplier to be applied to rotation while behavior is active")]
    [SerializeField]
    [Range(0f, 3f)]
    float chaseRotationSpeedup = 1f;

    [Tooltip("Minimum required distance between unit and target")]
    [SerializeField]
    float startDistance = 7f;

    [Tooltip("Distance from target at which behavior ends")]
    [SerializeField]
    float stopDistance = 2f;

    float sightTimer;
    SightedObject targetObject;


    void Update()
    {
        if (!IsActive)
            return;

        sightTimer += Time.deltaTime;


        if (targetObject == null)
        {
            EndBehavior(true, true);
        }


        if (targetObject.InSight)
        {
            sightTimer = 0f;

            if (m_Type == Chase.Type.LastKnownPosition)
            {
                m_Pathfinder.SetTarget(targetObject.LastKnownPosition);
            }
        }
        else if (MaximumTimeOutOfSight != -1 && sightTimer >= MaximumTimeOutOfSight)
        {
            if (m_Actor.ShowDebug)
            {
                Debug.Log(string.Format("CHASE --- {0} --- Max out of sight time reached: {1}.", m_Transform.name, sightTimer));
            }

            EndBehavior(true, true);
        }

    }



    public override void StartBehavior()
    {
        IsActive = true;

        //m_Movement.AddSpeedMultiplier(chaseMovementSpeedup);
        // m_Movement.AddRotationMultiplier(chaseRotationSpeedup);

        targetObject = m_Actor.TargetObject;
        sightTimer = 0f;

        if (targetObject != null)
        {
            switch (m_Type)
            {
                case Type.LastKnownPosition:
                    m_Pathfinder.SetAndSearch(targetObject.LastKnownPosition);
                    break;
                case Type.Transform:
                    m_Pathfinder.SetAndSearch(targetObject.SightedTransform);
                    break;
            }
        }
    }

    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

        //m_Movement.RemoveSpeedMultiplier(chaseMovementSpeedup);
        //m_Movement.RemoveRotationMultiplier(chaseRotationSpeedup);


        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        // throw new NotImplementedException();
    }



    public override float GetBehaviorScore()
    {
        if (m_Actor.TargetObject == null)
        {
            if (ShowDebug)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + ". Target Object is null");
            }

            return 0f;
        }
        if (m_Actor.TargetObject.SightedTransform == null)
        {
            if (ShowDebug)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + ". Target Object's SightedTransform is null");
            }

            return 0f;
        }

        if (!m_Actor.TargetObject.SightedTransform.gameObject.activeInHierarchy)
        {
            if (ShowDebug)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + ". Target Object's SightedTransform is not active");
            }

            return 0f;
        }

        if (MaximumTimeOutOfSight != -1 && !m_Actor.TargetObject.InSight)
        {
            if (ShowDebug)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + ". Target out of sight time exceeds chase time");
            }

            return 0f;
        }




        float dist = Vector3.Distance(m_Actor.TargetObject.LastKnownPosition, m_Transform.position);

        if (dist <= StopDistance)
        {
            return 0f;
        }


        return utilityCurve.Evaluate(Mathf.Clamp01(dist / StartDistance));
    }




    public override bool CanStartSubBehavior
    {
        get { return true; }
    }

    public float MaximumTimeOutOfSight
    {
        get { return maxTimeOutOfSight; }
        set
        {
            maxTimeOutOfSight = value;

            if (maxTimeOutOfSight < 0)
            {
                maxTimeOutOfSight = -1;
            }
        }
    }
    public float StopDistance
    {
        get { return stopDistance; }
        set { stopDistance = Mathf.Clamp(value, 0.5f, stopDistance); }
    }
    public float StartDistance
    {
        get { return startDistance; }
        set { startDistance = Mathf.Clamp(value, 0.5f, startDistance); }
    }


    protected override void OnValidate()
    {
        base.OnValidate();

        MaximumTimeOutOfSight = MaximumTimeOutOfSight;
        StopDistance = StopDistance;
        StartDistance = StartDistance;

        Utilities.ValidateCurve_Times(utilityCurve, 0f, 1f);
    }



    public override string ToString()
    {
        return "Chase";
    }


}
