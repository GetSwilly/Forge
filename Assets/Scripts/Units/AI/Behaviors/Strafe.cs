using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Strafe : BaseUtilityBehavior {
    

    [Tooltip("Used to determine volatility of strafe direction")]
    [SerializeField]
    float strafeValueDelta = 1f;

    [SerializeField]
    Vector2 radiusRange = new Vector2(4f, 7f);

    [SerializeField]
    float maxDistance = 5f;

    float currentStrafeValue = 0f;

    IMovement movementInterface;

    public override void Awake()
    {
        base.Awake();

        movementInterface = GetComponent<IMovement>();
    }


    IEnumerator StrafeBehavior()
    {
        SightedObject targetObject = m_Actor.TargetObject;

        if (m_Actor.ShowDebug)
        {
            Debug.Log(Time.time + " #### " + m_Transform.name + " -- Strafe -- Target: " + targetObject);
        }

        float travelledDistance = 0f;
        float desiredRadius = UnityEngine.Random.Range(RadiusRange.x, RadiusRange.y);

        while (IsActive)
        {
            if (targetObject.SightedTransform == null || !targetObject.SightedTransform.gameObject.activeInHierarchy || !targetObject.InSight)
            {
                EndBehavior(true, true);
            }

            
            float _delta = StrafeValueDelta * Time.deltaTime;
            _delta *= UnityEngine.Random.value < 0.5f ? -1f : 1f;

            currentStrafeValue += _delta;
            
            //if (m_Actor.ShowDebug)
            //{
            //    Debug.Log(Time.time + " #### " + m_Transform.name + " -- Strafe -- Strafing Dir: " + (strafeValue < 0 ? "Left": "Right"));
            //}

          
            movementInterface.RotateTowards(targetObject.LastKnownBasePosition);


            Vector3 localStrafeDirection = new Vector3(1,0,0);
            localStrafeDirection.y = 0;
            localStrafeDirection.Normalize();

            if (currentStrafeValue < 0)
            {
                localStrafeDirection *= -1;
            }

            Vector3 movementVector = m_Transform.TransformDirection(localStrafeDirection).normalized * movementInterface.Speed * Time.deltaTime  ;
            Vector3 position = m_Transform.position + movementVector;

            if (Vector3.Distance(position, targetObject.LastKnownBasePosition) < desiredRadius)
            {
                position = targetObject.LastKnownBasePosition + (position - targetObject.LastKnownBasePosition).normalized * desiredRadius;
            }

            if (!m_Pathfinder.CheckClearPath(position))
            {
                if (ShowDebug)
                {
                    //Debug.Log("")
                }
                break;
            }
            
            // m_Pathfinder.SetTarget(position);
            Vector3 originalPosition = m_Transform.position;

            movementInterface.MoveToPosition(position);

            travelledDistance += Vector3.Distance(originalPosition, m_Transform.position);

            if(travelledDistance >= MaxDistance)
            {
                if (ShowDebug)
                {
                    //Debug.Log("")
                }
                break;
            }
            

            yield return null;
        }

        EndBehavior(true,true);
    }


    public override void StartBehavior()
    {
        base.StartBehavior();

        currentStrafeValue = 0f;
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
    public Vector2 RadiusRange
    {
        get { return radiusRange; }
        private set
        {
            radiusRange = value;
            radiusRange.x = Mathf.Clamp(radiusRange.x, 0f, radiusRange.x);
            radiusRange.y = Mathf.Clamp(radiusRange.y, radiusRange.x, radiusRange.y);
        }
    }
    public float MaxDistance
    {
        get { return maxDistance; }
        private set { maxDistance = Mathf.Clamp(value, 0f, value); }
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        StrafeValueDelta = StrafeValueDelta;
        RadiusRange = RadiusRange;
        MaxDistance = MaxDistance;

        Utilities.ValidateCurve_Times(utilityCurve, 0f, 1f);
    }


    public override string ToString()
    {
        return "Strafe";
    }

 
}
