using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathfinding;

public class Wander : BaseUtilityBehavior
{

    static readonly float ANGLE_GROWTH_RATE = 25f;
    static readonly int MAX_ITERATIONS = 2;

    [Flags]
    public enum WanderType
    {
        Wander = 1,
        PatrolStartLevel = 1 << 1,
        PatrolEndLevel = 1 << 2,
        PatrolOrigin = 1 << 3
    }


    [Space(10)]

    [SerializeField]
    [EnumFlags]
    WanderType m_WanderType = WanderType.Wander;

    [Tooltip("Delay upon reaching wander destination")]
    [SerializeField]
    protected DeviatingFloat waitDelay;

    [Tooltip("Delay between updates to wander destination")]
    [SerializeField]
    protected DeviatingFloat updateDelay;

    [Tooltip("Relative maximum angle for deciding destination")]
    [SerializeField]
    [Range(0f, 180f)]
    float wanderAngle = 15f;

    [Tooltip("Maximum distance for a wander destination")]
    [SerializeField]
    protected float maxWanderDistance = 10f;

    [Tooltip("Multiplier to applied to speed while behavior is active")]
    [SerializeField]
    [Range(0f, 2f)]
    protected float wanderMoveSpeedup = 1f;

    [Tooltip("Multiplier to applied to rotation while behavior is active")]
    [SerializeField]
    [Range(0f, 2f)]
    protected float wanderRotateSpeedup = 1f;


    Transform patrolTarget;
    Vector3 patrolPosition;

 

    //void Update()
    //{
    //    if (!IsActive)
    //        return;


    //}

    private void PathFound(Path p)
    {
        //throw new NotImplementedException();
    }
    private void WaitAfterPathCompletion(Path p)
    {
        StartCoroutine(WaitDelay());
    }




    IEnumerator WaitDelay()
    {
        float waitTime = (float)Utilities.GetRandomGaussian(waitDelay);
        waitTime = Mathf.Max(0.1f, waitTime);

        yield return new WaitForSeconds(waitTime);

        Vector3 wanderPosition = FindWanderPosition();

        m_Pathfinder.SetAndSearch(wanderPosition);
    }

    Vector3 FindWanderPosition()
    {
        float _angle = UnityEngine.Random.Range(-wanderAngle, wanderAngle);

        Vector3 wanderDir = Quaternion.AngleAxis(_angle, m_Transform.up) * m_Transform.forward;

        Vector3 wanderPosition = GetWanderOrigin();
        wanderPosition += (wanderDir.normalized * maxWanderDistance);

        wanderPosition.y = m_Transform.position.y;


        return wanderPosition;
    }


    //IEnumerator WanderAround()
    //{
    //    patrolPosition = m_Transform.position;

    //    while (IsActive)
    //    {

    //        float t = 0;
    //        float waitTime = (float)Utilities.GetRandomGaussian(waitDelay);
    //        waitTime = Mathf.Max(0.1f, waitTime);



    //        yield return new WaitForSeconds(waitTime);


    //        Vector3 wanderPosition;
    //        Node wanderNode;
    //        int counter = 0;
    //        float maxAngle = wanderAngle;

    //        do
    //        {
    //            yield return null;

    //            counter++;
    //            maxAngle += Time.deltaTime * ANGLE_GROWTH_RATE;

    //            m_Actor.MoveAlongPath();

    //            float _angle = UnityEngine.Random.Range(0f, maxAngle);
    //            _angle *= UnityEngine.Random.value <= 0.5f ? 1f : -1f;

    //            Vector3 wanderDir = Quaternion.AngleAxis(_angle, m_Transform.up) * m_Transform.forward;

    //            wanderPosition = wanderDir.normalized * maxWanderDistance;

    //            wanderPosition += GetWanderOrigin();


    //            wanderPosition.y = m_Transform.position.y;
    //            wanderNode = A_Star_Pathfinding.Instance.WalkableNodeFromWorldPoint(wanderPosition, m_Actor.Bounds, m_Actor.WalkableNodes);
    //            // wanderNode = A_Star_Pathfinding.Instance.NodeFromWorldPoint(wanderPosition);  //.WalkableNodeFromWorldPoint(wanderPosition, myActor.Bounds, myActor.WalkableNodes);

    //        } while (counter < MAX_ITERATIONS && (wanderNode == null || !wanderNode.IsWalkable(m_Actor.WalkableNodes)));


    //        if (counter >= MAX_ITERATIONS)
    //        {
    //            EndBehavior(true, true);
    //            yield return null;
    //        }

    //        m_Actor.FindPathTo(wanderPosition);

    //        do
    //        {
    //            yield return null;
    //            m_Actor.MoveAlongPath();
    //        } while (m_Actor.SearchingForPath);

    //        float updateTime = (float)Utilities.GetRandomGaussian(updateDelay);
    //        updateTime = Mathf.Max(.05f, updateTime);

    //        t = 0;
    //        while (t < updateTime && m_Actor.MoveAlongPath())
    //        {
    //            yield return null;

    //            t += Time.deltaTime;
    //        }
    //    }


    //    yield return null;


    //    EndBehavior(true, true);
    //}



    private Vector3 GetWanderOrigin()
    {
        List<Vector3> potentialOrigins = new List<Vector3>();

        if (Utilities.HasFlag(m_WanderType, WanderType.PatrolStartLevel))
        {

        }


        if (Utilities.HasFlag(m_WanderType, WanderType.PatrolEndLevel))
        {
            potentialOrigins.Add(patrolTarget == null ? m_Transform.position : patrolTarget.position);
        }


        if (Utilities.HasFlag(m_WanderType, WanderType.PatrolOrigin))
        {
            potentialOrigins.Add(patrolPosition);
        }


        if (Utilities.HasFlag(m_WanderType, WanderType.Wander))
        {
            potentialOrigins.Add(m_Transform.position);
        }



        return potentialOrigins.Count == 0 ? m_Transform.position : potentialOrigins[UnityEngine.Random.Range(0, potentialOrigins.Count)];
    }

    private Transform GetTarget()
    {
        switch (m_WanderType)
        {
            case WanderType.PatrolStartLevel:
                return LevelController.Instance.StartGoalTransform;
            case WanderType.PatrolEndLevel:
                return LevelController.Instance.EndGoalTransform;
        }

        return null;
    }



    public override void StartBehavior()
    {
        IsActive = true;

        //m_Movement.AddSpeedMultiplier(wanderMoveSpeedup);
        //m_Movement.AddRotationMultiplier(wanderRotateSpeedup);
        
        PatrolTarget = GetTarget();

        //StartCoroutine(WanderAround());

        m_Pathfinder.OnPathFound += PathFound;
        m_Pathfinder.OnPathTraversalCompleted += WaitAfterPathCompletion;
    }


    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        //m_Movement.RemoveSpeedMultiplier(wanderMoveSpeedup);
        //m_Movement.RemoveRotationMultiplier(wanderRotateSpeedup);

        StopAllCoroutines();

        m_Pathfinder.StopPathTraversal();

        m_Pathfinder.OnPathFound -= PathFound;
        m_Pathfinder.OnPathTraversalCompleted -= WaitAfterPathCompletion;

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }


    public override float GetBehaviorScore()
    {
        return utilityCurve.Evaluate(UnityEngine.Random.value);
    }






    public override bool CanEndBehavior
    {
        get { return true; }
    }

    public override bool CanStartSubBehavior
    {
        get { return false; }
    }



    public Transform PatrolTarget
    {
        get { return patrolTarget; }
        private set { patrolTarget = value; }
    }



    public override string ToString()
    {
        return "Wander";
    }
}
