using UnityEngine;
using System.Collections;
using System;
using Pathfinding;

public class Flee : BaseUtilityBehavior
{
    //static readonly int MAX_ITERATIONS = 25;
    static readonly float SEARCH_ANGLE_DELTA = 15f;
    static readonly float MEAN_TO_OFFSET_RATIO = 3f;
    //static readonly float THREAT_POSITION_MIN_DISTANCE = 5f;

    [Tooltip("Determines how far the unit attempts to flee")]
    [SerializeField]
    protected DeviatingFloat fleeDistance;

    [Tooltip("Minimum distance that unit can flee")]
    [SerializeField]
    protected float minFleeDistance = 3f;


    [Tooltip("Multiplier applied to speed during fleeing")]
    [SerializeField]
    [Range(0.1f, 3f)]
    protected float fleeSpeedup = 1f;


    [Tooltip("Initial maximum angle for attempted flee")]
    [SerializeField]
    [Range(1, 180)]
    int initialFleeAngle = 30;
    
  


    public void FleeFromThreats()
    {
        Vector3 pos = FindThreatPosition();

        CalulcateFleeVector(pos);
    }
    public Vector3 FindThreatPosition()
    {
        //Attempt to continue moving in same direction
        Vector3 offset = -m_Pathfinder.Velocity;
        offset.y = 0f;

        //If not moving, construct own offset
        if (offset.magnitude <= 0.1f)
        {
            offset = Vector3.one * ( UnityEngine.Random.value < 0.5f ? 1 : -1);
        }

        offset.Normalize();


        Vector3 threatPos = m_Transform.position + offset;

        for (int i = 0; i < m_Actor.NearbyEnemies.Count; i++)
        {
            threatPos += m_Actor.NearbyEnemies[i].Transform.position;
        }

        threatPos /= m_Actor.NearbyEnemies.Count + 1;

        return threatPos;
    }
    public Vector3 CalulcateFleeVector(Vector3 threatPosition)
    {

        Vector3 fleeVector = (m_Transform.position - threatPosition).normalized;
        Vector3 perpendicularFleeVector = new Vector3(-fleeVector.z, fleeVector.y, fleeVector.x).normalized * ((UnityEngine.Random.value * 2f) - 1f);
        fleeVector += perpendicularFleeVector;

        Vector3 fleePos = m_Transform.position + (fleeVector.normalized * MEAN_TO_OFFSET_RATIO * (float)Utilities.GetRandomGaussian(fleeDistance));

        return fleePos;
    }





    void AttemptFlee()
    {
        Vector3 threatPos = FindThreatPosition();

        if (m_Actor.ShowDebug)
        {
            Debug.DrawLine(m_Transform.position, threatPos, Color.red, 2f);
        }

        Vector3 worldPosition = CalulcateFleeVector(threatPos);

        Vector3 zeroAngleVector = worldPosition - m_Transform.position;
        zeroAngleVector = zeroAngleVector.magnitude == 0 ? m_Transform.forward : zeroAngleVector.normalized;


        Vector3 toVector = worldPosition - m_Transform.position;
        toVector.y = 0;

        if (toVector.magnitude < minFleeDistance)
        {
            worldPosition = m_Transform.position + (toVector.normalized * minFleeDistance);
        }


        m_Pathfinder.SetAndSearch(worldPosition, CheckFleePath);
    }

 void CheckFleePath(Path p)
    {
        if (p.error)
        {
            EndBehavior(true, true);
        }

        m_Pathfinder.OnPathTraversalCompleted += FleeComplete;
    }


    void FleeComplete(Path p)
    {
        EndBehavior(true, true);
    }



    public override void StartBehavior()
    {
        IsActive = true;

        // m_Movement.AddSpeedMultiplier(fleeSpeedup);
        AttemptFlee();
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();
        //m_Movement.RemoveSpeedMultiplier(fleeSpeedup);
       
        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }


    public override float GetBehaviorScore()
    {
         if (m_Mind == null)
            return 0f;


        UtilityPersonalityTrait fearTrait = m_Mind.GetTrait(UtilityPersonalityTrait.PersonalityTrait_Type.Fear);

        if (fearTrait == null)
            return 0f;


        return utilityCurve.Evaluate(fearTrait.CurrentValue);
    }



    public override bool CanEndBehavior
    {
       get { return true; }
    }


    public override bool CanStartSubBehavior
    {
        get { return true; }
    }





    protected override void OnValidate()
    {
        base.OnValidate();

        Utilities.ValidateCurve_Times(utilityCurve, 0f, 100f);
    }

    public override string ToString()
    {
        return "Flee";
    }


}
