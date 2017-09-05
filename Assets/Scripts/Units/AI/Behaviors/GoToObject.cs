using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GoToObject : BaseUtilityBehavior
{

    [SerializeField]
    string targetTag = "";

    [SerializeField]
    AnimationClip myAnimationClip;


    Transform desiredTransform;
    
    

    IEnumerator MoveToTargetObject()
    {

        float updatePathTime = m_Actor.UpdatePathTime;


        //Loop while behavior and targetObject are active
        while (IsActive && desiredTransform != null && desiredTransform.gameObject.activeInHierarchy)
        {

            m_Actor.FindPathTo(desiredTransform);

            yield return null;



            //Move along path to targetObject, updating at regular intervals
            float timer = 0;

            while (timer <= updatePathTime && m_Actor.MoveAlongPath())
            {
                timer += Time.deltaTime;


                if (m_Actor.ShowDebug)
                    Debug.DrawLine(transform.position, desiredTransform.position, Color.yellow);


                yield return null;
            }

            //Break if targetObject not active AKA no longer available
            if (desiredTransform == null || !desiredTransform.gameObject.activeInHierarchy)
                break;



            //Check if timer ran out
            if (timer > updatePathTime)
                continue;



            //Set Path for last mile. AKA mve directly towards targetObject
            m_Actor.SetPath(new List<Vector3>() { desiredTransform.position });

            while (timer <= updatePathTime && m_Actor.MoveAlongPath())
            {
                timer += Time.deltaTime;


                if (m_Actor.ShowDebug)
                    Debug.DrawLine(transform.position, desiredTransform.position, Color.yellow);


                yield return null;
            }

            //Reached destination
            if (!m_Actor.MoveAlongPath())
            {
                //Perform animation;
                if (myAnimationClip != null)
                {
                    yield return new WaitForSeconds(myAnimationClip.length);
                }
                break;
            }
        }

        EndBehavior(true, true);
    }
    bool ChooseNearestObject()
    {
        List<Transform> nearbyTransforms = m_Actor.GetNearbyObject(targetTag);

        if (nearbyTransforms.Count == 0)
            return false;

        
        List<CustomTuple2<Transform, float>> viableObjects = new List<CustomTuple2<Transform, float>>();

        for (int i = 0; i < nearbyTransforms.Count; i++)
        {
            if (nearbyTransforms[i] == null || !nearbyTransforms[i].gameObject.activeInHierarchy)
                continue;


            float dist = A_Star_Pathfinding.Instance.EstimatePathDistance(transform.position, nearbyTransforms[i].position, Utilities.CalculateObjectBounds(gameObject, false), m_Actor.WalkableNodes);

            viableObjects.Add(new CustomTuple2<Transform, float>(nearbyTransforms[i], dist));
        }

        if (viableObjects.Count == 0)
            return false;


        viableObjects.Sort(delegate (CustomTuple2<Transform, float> x, CustomTuple2<Transform, float> y)
        {
            return y.Item2.CompareTo(x.Item2);
        });


        desiredTransform = viableObjects[0].Item1;

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
        desiredTransform = null;

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
        get { return true; }
    }


    public override string ToString()
    {
        return "GoToObject";
    }
}
