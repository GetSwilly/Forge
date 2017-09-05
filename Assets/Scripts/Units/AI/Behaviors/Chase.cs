using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(MovementController))]
public class Chase : BaseUtilityBehavior
{
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
    
    
    MovementController m_Movement;


    public override void Awake()
    {
        base.Awake();

        m_Movement = GetComponent<MovementController>();
    } 

    IEnumerator ChaseTarget()
    {
        float sightTimer = 0f;

        while (IsActive)
        {

            SightedObject targetObject = m_Actor.TargetObject;

            if (targetObject == null)
            {
                if (m_Actor.ShowDebug)
                {
                    Debug.Log(string.Format("CHASE --- {0} --- Target is null.", m_Transform.name));
                }

                break;
            }

            if (!targetObject.SightedTransform.gameObject.activeInHierarchy)
            {
                if (m_Actor.ShowDebug)
                {
                    Debug.Log(string.Format("CHASE --- {0} --- Target not active.", m_Transform.name));
                }

                    break;
            }

            if (MaximumTimeOutOfSight >= 0 && !targetObject.InSight)
            {
                if (m_Actor.ShowDebug)
                {
                    Debug.Log(string.Format("CHASE --- {0} --- Target not in sight.", m_Transform.name));
                }

                break;
            }
            


            m_Actor.FindPathTo(targetObject.LastKnownPosition);

            yield return null;


            float timer = 0;


            while (timer <= m_Actor.UpdatePathTime && m_Actor.MoveAlongPath())
            {

                if (m_Actor.ShowDebug)
                {
                    Debug.DrawLine(m_Transform.position, targetObject.LastKnownPosition, Color.cyan);
                }

                m_Movement.RotateTowards(targetObject.LastKnownPosition);

                sightTimer += Time.deltaTime;
                timer += Time.deltaTime;
                
                Vector3 toTarget = targetObject.LastKnownPosition - m_Transform.position;


                //Stop chasing if close enough
                if (toTarget.magnitude <= StopDistance)
                {
                    if (m_Actor.ShowDebug)
                    {
                        Debug.Log(string.Format("CHASE --- {0} --- Within stop distance: {1}.", m_Transform.name, toTarget.magnitude));
                    }

                    EndBehavior(true, true);
                }
                

                //End behavior if lost sight of target
                if (toTarget.magnitude <= m_Actor.SightRange)
                {
                    sightTimer = 0f;
                }
                else if(MaximumTimeOutOfSight != -1 && sightTimer >= maxTimeOutOfSight)
                {
                    if (m_Actor.ShowDebug)
                    {
                        Debug.Log(string.Format("CHASE --- {0} --- Max out of sight time reached: {1}.", m_Transform.name, sightTimer));
                    }

                    EndBehavior(true, true);
                }


                yield return null;
            }
        }

        EndBehavior(true, true);
    }



    public override void StartBehavior()
    {
        IsActive = true;

        m_Movement.AddSpeedMultiplier(chaseMovementSpeedup);
        m_Movement.AddRotationMultiplier(chaseRotationSpeedup);

        StartCoroutine(ChaseTarget());
    }

    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

        m_Movement.RemoveSpeedMultiplier(chaseMovementSpeedup);
        m_Movement.RemoveRotationMultiplier(chaseRotationSpeedup);


        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }
     
    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
       // throw new NotImplementedException();
    }


    public override float GetBehaviorScore()
    {
        if (m_Actor.TargetObject == null || m_Actor.TargetObject.SightedTransform == null || !m_Actor.TargetObject.SightedTransform.gameObject.activeInHierarchy || (MaximumTimeOutOfSight !=-1 && !m_Actor.TargetObject.InSight))
            return 0f;



        float dist = Vector3.Distance(m_Actor.TargetObject.LastKnownPosition, m_Transform.position);

        if(dist <= StopDistance)
        {
            return 0f;
        }


        return utilityCurve.Evaluate(Mathf.Clamp01(dist/StartDistance));
    }


    

    public override bool CanStartSubBehavior
    {
       get{ return true; }
    }
    
    public float MaximumTimeOutOfSight
    {
        get { return maxTimeOutOfSight; }
        set
        {
            maxTimeOutOfSight = value;

            if(maxTimeOutOfSight < 0)
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
