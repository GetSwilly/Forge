using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Strafe : BaseUtilityBehavior {
    

    [Tooltip("Used to determine volatility of strafe direction")]
    [SerializeField]
    float strafeValueDelta = 1f;

    float strafeValue = 0f;
    
    MovementController m_Movement;

    public override void Awake()
    {
        base.Awake();

        m_Movement = GetComponent<MovementController>();
    }





    IEnumerator StrafeBehavior()
    {
        SightedObject targetObject = m_Actor.TargetObject;

        if (m_Actor.ShowDebug)
        {
            Debug.Log(Time.time + " #### " + m_Transform.name + " -- Strafe -- Target: " + targetObject);
        }


        while (IsActive)
        {
            if (targetObject.SightedTransform == null || !targetObject.SightedTransform.gameObject.activeInHierarchy || !targetObject.InSight)
            {
                EndBehavior(true, true);
            }

            
            float _delta = StrafeValueDelta * Time.deltaTime;
            _delta *= UnityEngine.Random.value < 0.5f ? -1f : 1f;

            strafeValue += _delta;


            Vector3 circleDir = m_Transform.right;
            circleDir.y = 0;
            circleDir.Normalize();

            if (strafeValue < 0)
            {
                circleDir *= -1;
            }


            //if (m_Actor.ShowDebug)
            //{
            //    Debug.Log(Time.time + " #### " + m_Transform.name + " -- Strafe -- Strafing Dir: " + (strafeValue < 0 ? "Left": "Right"));
            //}


            m_Movement.MoveInLocalDirection(circleDir);
            m_Movement.RotateTowards(targetObject.LastKnownBasePosition);



            yield return null;
        }

        EndBehavior(true,true);
    }



    public override void StartBehavior()
    {
        IsActive = true;

        strafeValue = 0f;
        StartCoroutine(StrafeBehavior());
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();
        
        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

    public override float GetBehaviorScore()
    {
        if (m_Actor.TargetObject == null)
            return 0f;

        return utilityCurve.Evaluate(UnityEngine.Random.value);
    }


    
    public override bool CanEndBehavior
    {
        get{ return true; }
    }


    public override bool CanStartSubBehavior
    {
        get { return true; }
    }
    



    public float StrafeValueDelta
    {
        get { return strafeValueDelta; }
        set { strafeValueDelta = Mathf.Clamp(value, 0f, strafeValueDelta); }
    }


    protected override void OnValidate()
    {
        base.OnValidate();

        StrafeValueDelta = StrafeValueDelta;
        Utilities.ValidateCurve_Times(utilityCurve, 0f, 1f);
    }


    public override string ToString()
    {
        return "Strafe";
    }

 
}
