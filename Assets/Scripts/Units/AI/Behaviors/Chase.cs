using UnityEngine;
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

    //[SerializeField]
    //float slowdownDistance = 4f;

    float sightTimer;
    SightedObject targetObject;

    //float originalEndReachedDistance;
    //float originalSlowdownDistance;

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
                m_Pathfinder.SetTarget(targetObject.LastKnownBasePosition);
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

        if (m_Pathfinder.GetDistanceRemaining() < StopDistance)
        {
            if (m_Actor.ShowDebug)
            {
                Debug.Log(string.Format("CHASE --- {0} --- Within StoppingDistance.", m_Transform.name));
            }

            EndBehavior(true, true);
        }


        if (ShowDebug)
        {
            Debug.DrawLine(m_Transform.position, targetObject.LastKnownBasePosition, Color.yellow);
        }
        //if(m_Pathfinder.GetDistanceRemaining() < m_Actor.SightRange)
        //{
        //    m_Pathfinder.ShouldRotateTowardsPath = false;
        //    m_Pathfinder.RotateTowards(targetObject.LastKnownBasePosition);
        //}
        //else
        //{
        //    m_Pathfinder.ShouldRotateTowardsPath = true;
        //}
    }



    public override void StartBehavior()
    {
        IsActive = true;

        //originalEndReachedDistance = m_Pathfinder.EndReachedDistance;
        //m_Pathfinder.EndReachedDistance = StopDistance;

        //originalSlowdownDistance = m_Pathfinder.SlowdownDistance;
        //m_Pathfinder.SlowdownDistance = slowdownDistance;
        //m_Movement.AddSpeedMultiplier(chaseMovementSpeedup);
        // m_Movement.AddRotationMultiplier(chaseRotationSpeedup);

        targetObject = m_Actor.TargetObject;
        sightTimer = 0f;

        if (targetObject != null)
        {
            switch (m_Type)
            {
                case Type.LastKnownPosition:
                    m_Pathfinder.SetAndSearch(targetObject.LastKnownBasePosition);
                    break;
                case Type.Transform:
                    m_Pathfinder.SetAndSearch(targetObject.SightedTransform);
                    break;
            }
        }
    }

    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        if (!IsActive)
            return;

        StopAllCoroutines();

        //m_Movement.RemoveSpeedMultiplier(chaseMovementSpeedup);
        //m_Movement.RemoveRotationMultiplier(chaseRotationSpeedup);

        // m_Pathfinder.ShouldRotateTowardsPath = true;
        //m_Pathfinder.EndReachedDistance = originalEndReachedDistance;
        m_Pathfinder.StopPathTraversal();

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




        float dist = Vector3.Distance(m_Actor.TargetObject.LastKnownBasePosition, m_Transform.position);

        if (dist <= StartDistance)
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
    //public float SlowdownDistance
    //{
    //    get { return slowdownDistance; }
    //    set { slowdownDistance = Mathf.Clamp(value, 0f, value); }
    //}

    protected override void OnValidate()
    {
        base.OnValidate();

        MaximumTimeOutOfSight = MaximumTimeOutOfSight;
        StopDistance = StopDistance;
        StartDistance = StartDistance;
        //SlowdownDistance = SlowdownDistance;

        Utilities.ValidateCurve_Times(utilityCurve, 0f, 1f);
    }



    public override string ToString()
    {
        return "Chase";
    }


}
