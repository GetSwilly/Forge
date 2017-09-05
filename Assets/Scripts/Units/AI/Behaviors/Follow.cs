using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(MovementController))]
public class Follow : BaseUtilityBehavior {
    
   [Tooltip("Multplier applied to speed while behavior is active")]
    [SerializeField]
    [Range(0f, 3f)]
    float followSpeedup = 1f;

    [Tooltip("Mimimum required distance between unit and target")]
    [SerializeField]
    float startDistance = 7f;

    [Tooltip("Distance between unit and target at which behavior ends")]
    [SerializeField]
    float stopDistance = 2f;



    float updatePathTime;
    
    MovementController m_Movement;


    public override void Awake()
    {
        base.Awake();

        m_Movement = GetComponent<MovementController>();
    }
    void Start()
    {
        updatePathTime = m_Actor.UpdatePathTime;
    }


    IEnumerator FollowObject()
    {
        while (IsActive)
        {

            SightedObject followObject = m_Actor.FollowTarget;


            if (followObject == null || !followObject.SightedTransform.gameObject.activeInHierarchy || !followObject.InSight)
                break;



            m_Actor.FindPathTo(followObject.LastKnownPosition);

            yield return null;


            float timer = 0;


            while (timer <= updatePathTime && m_Actor.MoveAlongPath())
            {

                if (m_Actor.ShowDebug)
                {
                    Debug.DrawLine(m_Transform.position, followObject.LastKnownPosition, Color.green);
                }

                m_Movement.RotateTowards(followObject.LastKnownPosition);
                
                timer += Time.deltaTime;

                Vector3 toTarget = followObject.LastKnownPosition - m_Transform.position;


                //Stop chasing if close enough
                if (toTarget.magnitude <= stopDistance)
                {
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

        m_Movement.AddSpeedMultiplier(followSpeedup);
        StartCoroutine(FollowObject());
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

        m_Movement.RemoveSpeedMultiplier(followSpeedup);


        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }


    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

    public override float GetBehaviorScore()
    {
        if (m_Actor.FollowTarget == null || !m_Actor.FollowTarget.SightedTransform.gameObject.activeInHierarchy)
            return 0f;

        float dist = Vector3.Distance(m_Actor.FollowTarget.LastKnownPosition, m_Transform.position);


        return utilityCurve.Evaluate(Mathf.Clamp01(dist / startDistance));
    }


    public override bool CanStartSubBehavior
    {
        get{ return true; }
    }

   

   

    

    public override string ToString()
    {
        return "Follow";
    }

}
