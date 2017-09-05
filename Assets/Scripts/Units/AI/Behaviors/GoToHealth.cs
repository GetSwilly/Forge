using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Health))]
public class GoToHealth :  BaseUtilityBehavior {


    GameObject desiredHealthObject;
    
    Health m_health;

    public override void Awake()
    {
        base.Awake();

        m_health = GetComponent<Health>();
    }


    IEnumerator MoveToTargetObject()
    {

        float updatePathTime = m_Actor.UpdatePathTime;


        //Loop while behavior and targetObject are active
        while (IsActive && desiredHealthObject != null && desiredHealthObject.activeInHierarchy)
        {

            m_Actor.FindPathTo(desiredHealthObject.transform);

            yield return null;



            //Move along path to targetObject, updating at regular intervals
            float timer = 0;

            while (timer <= updatePathTime && m_Actor.MoveAlongPath())
            {
                timer += Time.deltaTime;


                if (m_Actor.ShowDebug)
                    Debug.DrawLine(transform.position, desiredHealthObject.transform.position, Color.yellow);


                yield return null;
            }

            //Break if targetObject not active AKA no longer available
            if (desiredHealthObject == null || !desiredHealthObject.activeInHierarchy)
                break;



            //Check if timer ran out
            if (timer > updatePathTime)
                continue;



            //Set Path for last mile. AKA mve directly towards targetObject
            m_Actor.SetPath(new List<Vector3>() { desiredHealthObject.transform.position });

            while (timer <= updatePathTime && m_Actor.MoveAlongPath())
            {
                timer += Time.deltaTime;


                if (m_Actor.ShowDebug)
                    Debug.DrawLine(transform.position, desiredHealthObject.transform.position, Color.yellow);


                yield return null;
            }


        }

        EndBehavior(true,true);
    }
    bool ChooseNearestObject()
    {
        List<Transform> nearbyHealthObjects = m_Actor.NearbyHealth;  //=UnitController.GetNearbyHealth();   //Get Nearby Health objects

        if (nearbyHealthObjects.Count == 0)
            return false;


        float healthDifference = m_health.MaxHealth - m_health.CurHealth;

        List<CustomTuple2<Transform, float>> viableObjects = new List<CustomTuple2<Transform, float>>();

        for (int i = 0; i < nearbyHealthObjects.Count; i++)
        {

            //Is Object now null?
            if (nearbyHealthObjects[i] == null)
            {
                nearbyHealthObjects.RemoveAt(i);
                i--;
                continue;
            }
            IHealthProvider providerScript = nearbyHealthObjects[i].GetComponent<IHealthProvider>();

            if (providerScript == null)
                continue;


            float dist = A_Star_Pathfinding.Instance.EstimatePathDistance(transform.position, nearbyHealthObjects[i].position, Utilities.CalculateObjectBounds(gameObject, false), m_Actor.WalkableNodes);

            if (dist == 0)
                dist = 1;



            float val = providerScript.HealthValue;

            if (val <= 0)
                continue;



            val = val - healthDifference;
            val /= dist;

            viableObjects.Add(new CustomTuple2<Transform, float>(nearbyHealthObjects[i], val));
        }

        if (viableObjects.Count == 0)
            return false;


        viableObjects.Sort(delegate (CustomTuple2<Transform, float> x, CustomTuple2<Transform, float> y)
        {
            return y.Item2.CompareTo(x.Item2);
        });


        desiredHealthObject = viableObjects[0].Item1.gameObject;

        return true;
    }



    public override void StartBehavior()
    {
        IsActive = true;

        if (!ChooseNearestObject())
        {
            EndBehavior(true,true);
        }
        else
        {
            StartCoroutine(MoveToTargetObject());
        }

    }
    
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        IsActive = false;


        StopAllCoroutines();
        desiredHealthObject = null;

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }


    public override float GetBehaviorScore()
    {

        if (m_health == null || m_Actor.NearbyHealth.Count == 0)
            return 0f;

        return utilityCurve.Evaluate(m_health.HealthPercentage);
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
        return "GoToHealth";
    }
}
