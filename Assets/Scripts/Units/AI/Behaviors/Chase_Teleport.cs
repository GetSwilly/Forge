using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathfinding;

[RequireComponent(typeof(Chase))]
[RequireComponent(typeof(Rigidbody))]
public class Chase_Teleport : BaseUtilityBehavior {

    static readonly float TELEPORT_SPEEDUP = 10f;
    static readonly int MAX_ITERATIONS = 50;

    [Tooltip("Minimum distance between target and teleport destination")]
    [SerializeField]
    float minDistanceFromTarget = 1.5f;

    [Tooltip("Relative maximum angle from which teleport destination can be chosen")]
    [SerializeField]
    [Range(0f, 90f)]
    float maxTeleportAngle = 45f;

    [Tooltip("Distance between unit and teleportation target")]
    [SerializeField]
    DeviatingFloat teleportDistance;

    [SerializeField]
    AudioClip teleportSound;
    
    
    Rigidbody m_Rigidbody;

    Collider[] m_Colliders;
    Renderer[] m_Renderers;

    public override void Awake()
    {
        base.Awake();

        m_Rigidbody = GetComponent<Rigidbody>();

        m_Colliders = GetComponents<Collider>();
        m_Renderers = GetComponents<Renderer>();
    }
    


    void ActivateSpectral()
    {
        for (int i = 0; i < m_Colliders.Length; i++)
        {
            m_Colliders[i].enabled = false;
        }

        for (int i = 0; i < m_Renderers.Length; i++)
        {
            Color c = m_Renderers[i].material.color;
            c.a = 0.5f;
            m_Renderers[i].material.color = c;
        }

        m_Rigidbody.isKinematic = true;
    }
    void DeactivateSpectral()
    {
        for (int i = 0; i < m_Colliders.Length; i++)
        {
            m_Colliders[i].enabled = true;
        }

        for (int i = 0; i < m_Renderers.Length; i++)
        {
            //myRenderers[i].enabled = true;

            Color c = m_Renderers[i].material.color;
            c.a = 1f;
            m_Renderers[i].material.color = c;
        }

        m_Rigidbody.isKinematic = false;
    }
   


    void AttemptTeleport()
    {

        SightedObject target = m_Actor.TargetObject;

        if (target == null || !target.SightedTransform.gameObject.activeInHierarchy)
        {
            Debug.Log("Null Target");
            EndBehavior(true, true);
        }

    


            if (target == null || !target.SightedTransform.gameObject.activeInHierarchy)
            {
                Debug.Log("Null Target");
                EndBehavior(true, true);
            }



        Vector3 toTarget = target.LastKnownBasePosition - m_Transform.position;


        Vector3 teleportPos = UnityEngine.Random.insideUnitSphere;



            if(Vector3.Angle(teleportPos.normalized, toTarget) > maxTeleportAngle)
        {
            EndBehavior(true, true);
        }
            

            float dist = (float)Utilities.GetRandomGaussian(teleportDistance);


            teleportPos.Normalize();
            teleportPos *= dist;
            teleportPos += m_Transform.position;

        if(Vector3.Distance(teleportPos, target.LastKnownBasePosition) < minDistanceFromTarget)
        {
            EndBehavior(true, true);
            return;
        }

        m_Pathfinder.SearchPath(teleportPos, CheckValidTeleport);
    }


    void CheckValidTeleport(Path p)
    {
        if (!IsActive)
            return;

        if (p.error)
        {
            EndBehavior(true, true);
        }

        m_Pathfinder.CanMove = false;

       Vector3 pos = (Vector3)p.path[p.path.Count-1].position;


        ActivateSpectral();

        SightedObject target = m_Actor.TargetObject;

        while (target != null && target.SightedTransform.gameObject.activeInHierarchy && Vector3.Distance(m_Transform.position, pos) > 0.01f)
        {
            m_Transform.position = Vector3.MoveTowards(m_Transform.position, pos, m_Pathfinder.Speed * TELEPORT_SPEEDUP * Time.deltaTime);
            //m_Movement.RotateTowards(target.LastKnownPosition);
        }


        EndBehavior(true, true);
    }


    public override void StartBehavior()
    {
        base.StartBehavior();

        AttemptTeleport();
    }
   
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

        m_Pathfinder.CanMove = true;
        DeactivateSpectral();

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }
    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }


    public override float GetBehaviorScore()
    {
        Chase _chase = GetComponent<Chase>();

        if (_chase == null || !_chase.IsActive)
            return 0f;

        return utilityCurve.Evaluate(UnityEngine.Random.value);
    }

    public override bool CanStartBehavior
    {
        get
        {
            Chase _chase = GetComponent<Chase>();

            if (_chase == null || !_chase.IsActive)
                return false;

            return base.CanStartBehavior;
        }
    }

   

    public override bool CanStartSubBehavior
    {
        get
        {
            if (subBehavior == null)
                return true;


            return subBehavior.CanEndBehavior || subBehavior.CanStartBehavior;
        }
    }
    
    
    public override string ToString()
    {
        return "Chase_Teleport";
    }
}
