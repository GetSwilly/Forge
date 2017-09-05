using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MovementController))]
public class SearchForTarget : BaseUtilityBehavior
{
    static readonly float ANGLE_GROWTH_RATE = 15f;
    static readonly float ACCEPTANCE_ANGLE = 3f;
    static readonly int MAX_ITERATIONS = 1000;



   // [SerializeField]
   // float updateTimer = 1f;


    //Search variables
    [SerializeField]
    [Range(0f, 180f)]
    float maxSearchAngle = 40f;

    [SerializeField]
    float searchDistanceMean = 8f;

    [SerializeField]
    float searchDistanceSigma = 0f;

    [SerializeField]
    [Range(0f, 1f)]
    float searchMovementSpeedup = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    float searchRotateSpeedup = 1f;


    //Scan variables
    [SerializeField]
    float scanTimeMean = 4f;

    [SerializeField]
    float scanTimeSigma = 0f;

    [SerializeField]
    float scanWaitMean = 4f;

    [SerializeField]
    float scanWaitSigma = 0f;

    [SerializeField]
    [Range(0f, 180f)]
    float maxScanAngle = 25f;

    [SerializeField]
    [Range(0f, 1f)]
    float scanMovementSpeedup = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    float scanRotateSpeedup = 1f;


    float activeMovementSpeedup = 1f;
    float activeRotateSpeedup = 1f;

    MovementController m_Movement;

    public override void Awake()
    {
        base.Awake();

        m_Movement = GetComponent<MovementController>();
    }





    IEnumerator SearchRoutine()
    {
        SightedObject searchTarget = m_Actor.TargetObject;
        Vector3 searchDir = searchTarget.LastKnownDirection;



        while (m_Actor.TargetObject == searchTarget && !searchTarget.InSight && searchTarget.SightedTransform != null && searchTarget.SightedTransform.gameObject.activeInHierarchy)
        {
            float localUpdateTimer = 0;

            float localScanTime = (float)Utilities.GetRandomGaussian(scanTimeMean, scanTimeSigma);
            localScanTime = Mathf.Max(0f, localScanTime);
            

            Vector3 scanLookPosition = m_Transform.position + m_Transform.forward;


            m_Movement.AddSpeedMultiplier(scanMovementSpeedup);
            m_Movement.AddRotationMultiplier(scanRotateSpeedup);
            activeMovementSpeedup = scanMovementSpeedup;
            activeRotateSpeedup = scanRotateSpeedup;

            //Scan for SearchTarget
            while(localUpdateTimer < localScanTime)
            {
                yield return null;

                localUpdateTimer += Time.deltaTime;


                //Rotate to new vector
                m_Movement.RotateTowards(scanLookPosition);
                if(Vector3.Angle(m_Transform.forward, scanLookPosition - m_Transform.position) <= ACCEPTANCE_ANGLE)
                {

                    float localWaitTime = (float)Utilities.GetRandomGaussian(scanWaitMean, scanWaitSigma);
                    localWaitTime = Mathf.Max(0f, localWaitTime);

                    yield return new WaitForSeconds(localWaitTime);
                    localUpdateTimer += localWaitTime;

                    float _angle = UnityEngine.Random.Range(0f, maxScanAngle);
                    _angle *= UnityEngine.Random.value <= 0.5f ? 1f : -1f;

                    scanLookPosition = (Quaternion.AngleAxis(_angle, m_Transform.up) * searchDir).normalized + m_Transform.position;
                }
            }


            m_Movement.RemoveSpeedMultiplier(scanMovementSpeedup);
            m_Movement.RemoveRotationMultiplier(scanRotateSpeedup);
            activeMovementSpeedup = 1f;
            activeRotateSpeedup = 1f;

            //    if (localUpdateTimer > updateTimer)
            //       continue;

            //Go off in search of Target
            Vector3 searchPosition;
            Node desiredNode;
            int counter = 0;
            float maxAngle = maxSearchAngle;

            do
            {
                yield return null;

                counter++;
                maxAngle += Time.deltaTime * ANGLE_GROWTH_RATE;

                m_Actor.MoveAlongPath();

                float _angle = UnityEngine.Random.Range(0f, maxAngle);
                _angle *= UnityEngine.Random.value <= 0.5f ? 1f : -1f;

                Vector3 newSearchDir = Quaternion.AngleAxis(_angle, m_Transform.up) * searchDir;


                float searchDist = (float)Utilities.GetRandomGaussian(searchDistanceMean, searchDistanceSigma);
                searchDist = Mathf.Max(0f, searchDist);


                searchPosition = m_Transform.position + (newSearchDir.normalized * searchDist);
                searchPosition.y = m_Transform.position.y;


                desiredNode = A_Star_Pathfinding.Instance.WalkableNodeFromWorldPoint(searchPosition , m_Actor.Bounds, m_Actor.WalkableNodes);

            } while (counter < MAX_ITERATIONS && (desiredNode == null || !desiredNode.IsWalkable(m_Actor.WalkableNodes)));
            

            if (counter >= MAX_ITERATIONS)
            {
                break;
            }

        //    if (localUpdateTimer > updateTimer)
          //      continue;

            searchDir = searchPosition - m_Transform.position;
            m_Actor.FindPathTo(searchPosition);

            m_Movement.AddSpeedMultiplier(searchMovementSpeedup);
            m_Movement.AddRotationMultiplier(searchRotateSpeedup);
            activeMovementSpeedup = searchMovementSpeedup;
            activeRotateSpeedup = searchRotateSpeedup;



            while (m_Actor.MoveAlongPath()){
                yield return null;
            }

            m_Movement.RemoveSpeedMultiplier(searchMovementSpeedup);
            m_Movement.RemoveRotationMultiplier(searchRotateSpeedup);
            activeMovementSpeedup = 1f;
            activeRotateSpeedup = 1f;
        }


        EndBehavior(true, true);
    }




    public override void StartBehavior()
    {
        IsActive = true;

        activeMovementSpeedup = 1f;
        activeRotateSpeedup = 1f;

        StartCoroutine(SearchRoutine());
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

        m_Movement.RemoveSpeedMultiplier(activeMovementSpeedup);
        m_Movement.RemoveRotationMultiplier(activeRotateSpeedup);

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }


    public override bool CanStartSubBehavior
    {
        get { return false; }
    }

    public override float GetBehaviorScore()
    {
        SightedObject searchObj = m_Actor.TargetObject;

        if (searchObj == null || searchObj.InSight)
            return 0f;


        return utilityCurve.Evaluate(Mathf.Clamp01((Time.time - searchObj.LastTimeSeen) / m_Actor.MemoryTime));
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

 

    public override string ToString()
    {
        return "Search For Target";
    }
}
