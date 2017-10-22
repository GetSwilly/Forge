using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceTarget : BaseUtilityBehavior
{
    [SerializeField]
    bool requireLOS = true;


    IEnumerator FaceTargetRoutine()
    {
        while (IsActive)
        {
            if (m_Actor.TargetObject == null)
            {
                if (ShowDebug)
                {
                    Debug.Log("FaceTarget -- Target object is null");
                }
                break;
            }

            if (requireLOS && !m_Actor.TargetObject.InSight)
            {
                if (ShowDebug)
                {
                    Debug.Log("FaceTarget -- LSO required and Target object is not in sight.");
                }
                break;
            }

            if (ShowDebug)
            {
                Debug.DrawLine(m_Transform.position, m_Actor.TargetObject.LastKnownBasePosition);
            }

            m_Pathfinder.RotateTowards(m_Actor.TargetObject.LastKnownBasePosition);
            yield return null;
        }

        EndBehavior(true, true);
    }

    public override void StartBehavior()
    {
        base.StartBehavior();
     
        StartCoroutine(FaceTargetRoutine());
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }
    public override float GetBehaviorScore()
    {
        if (m_Actor.TargetObject == null)
            return 0f;

        Vector3 targetPos = m_Actor.TargetObject.LastKnownBasePosition;

        Vector3 toTarget = targetPos - m_Transform.position;
        toTarget.y = 0;

        float angle = Vector3.Angle(new Vector3(m_Transform.forward.x, 0f, m_Transform.forward.z), toTarget);
        float score = utilityCurve.Evaluate(angle);
        if (ShowDebug)
        {
            Debug.Log("FaceTarget -- Angle: " + angle + ". Score: " + score);
        }

        return score;
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }
    public override bool CanStartSubBehavior
    {
        get
        {
            return base.CanStartBehavior && m_Actor.TargetObject != null;
        }
    }

    public override string ToString()
    {
        return "Face Target";
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        Utilities.ValidateCurve(utilityCurve, 0f, 180f, 0f, 100f);
    }
}
