using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(MovementController))]
public class Observe : BaseUtilityBehavior
{

    SightedObject targetObject;
    
    MovementController m_Movement;

    public override void Awake()
    {
        base.Awake();

        m_Movement = GetComponent<MovementController>();
    }

    IEnumerator FaceTarget()
    {
        while (IsActive && targetObject.InSight && targetObject.SightedTransform != null && targetObject.SightedTransform.gameObject.activeInHierarchy)
        {
            m_Movement.RotateTowards(targetObject.LastKnownPosition);
            yield return null;
        }

        EndBehavior(true, true);
    }
    void ChooseTarget()
    {
        targetObject = m_Actor.TargetObject;
    }




    public override void StartBehavior()
    {
        IsActive = true;

        ChooseTarget();
        StartCoroutine(FaceTarget());
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }
    public override bool CanStartSubBehavior
    {
        get { return true; }
    }

    public override float GetBehaviorScore()
    {
        throw new NotImplementedException();
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

   

    public override string ToString()
    {
        return "Observe";
    }
}
