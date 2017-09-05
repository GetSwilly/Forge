using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GoToPickup : BaseUtilityBehavior {

    [SerializeField]
    ItemPickup.PickupType m_Type;
   

    GameObject targetObject;


    IEnumerator MoveToTargetObject()
    {

        m_Actor.FindPathTo(targetObject.transform);

        yield return null;

        while (m_Actor.MoveAlongPath())
        {
            yield return null;
        }


        EndBehavior(true, true);
    }

    bool ChooseNearestObject()
    {
        List<Transform> nearbyPickups;

        if (m_Type == ItemPickup.PickupType.Ability)
        {
            nearbyPickups = m_Actor.NearbyAbilities;
        }
        else
        {
            nearbyPickups = m_Actor.NearbyWeapons;
        }

        

        Transform nearestObject = null; // nearbyPickups[0];
        float shortestDist = float.MaxValue; // A_Star_Pathfinding.Instance.EstimatePathDistance(transform.position, nearestObject.position, Utilities.CalculateObjectBounds(gameObject, false), myActor.WalkableNodes);

        for (int i = 0; i < nearbyPickups.Count; i++)
        {
            if (!nearbyPickups[i].gameObject.activeInHierarchy)
                continue;


            float dist = A_Star_Pathfinding.Instance.EstimatePathDistance(transform.position, nearbyPickups[i].position, Utilities.CalculateObjectBounds(gameObject, false), m_Actor.WalkableNodes);

            if (dist > shortestDist)
                continue;

            nearestObject = nearbyPickups[i];
            shortestDist = dist;
        }

        if (nearestObject == null)
            return false;


        targetObject = nearestObject.gameObject;

        return true;
    }



    public override void StartBehavior()
    {
        IsActive = true;

        if (!ChooseNearestObject())
        {
            EndBehavior(true, true);
        }
        else
        {
            StartCoroutine(MoveToTargetObject());
        }

    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();
        targetObject = null;

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }


    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }


    public override float GetBehaviorScore()
    {
        Health _health = GetComponent<Health>();

        if (_health == null)
            return 0f;

        return utilityCurve.Evaluate(_health.HealthPercentage);
    }


    public override bool CanEndBehavior
    {
        get { return true; }
    }



    public override bool CanStartSubBehavior
    {
        get { return false; }
    }

   



    public override string ToString()
    {
        return "GoToPickup";
    }

    
}
