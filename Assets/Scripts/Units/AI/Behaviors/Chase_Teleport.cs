using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Chase))]
[RequireComponent(typeof(MovementController))]
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
    MovementController m_Movement;

    Collider[] m_Colliders;
    Renderer[] m_Renderers;

    public override void Awake()
    {
        base.Awake();

        m_Rigidbody = GetComponent<Rigidbody>();
        m_Movement = GetComponent<MovementController>();

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
   
    IEnumerator TeleportToTarget()
    {

        SightedObject target = m_Actor.TargetObject;

        if (target == null || !target.SightedTransform.gameObject.activeInHierarchy)
        {
            Debug.Log("Null Target");
            EndBehavior(true, true);
        }


        Vector3 toTarget;
        Vector3 teleportPos = m_Transform.position;
        Node _node;

        bool isValid = false;
        int iterationCounter = 0;

        do
        {
            if (iterationCounter >= MAX_ITERATIONS)
            {
                iterationCounter = 0;
                yield return null;
            }


            if (target == null || !target.SightedTransform.gameObject.activeInHierarchy)
            {
                Debug.Log("Null Target");
                EndBehavior(true, true);
                break;
            }


            isValid = true;


            toTarget = target.LastKnownPosition - m_Transform.position;


            teleportPos = UnityEngine.Random.insideUnitSphere;



            isValid = Vector3.Angle(teleportPos.normalized, toTarget) <= maxTeleportAngle;

            if (!isValid)
                continue;


            float dist = (float)Utilities.GetRandomGaussian(teleportDistance);


            teleportPos.Normalize();
            teleportPos *= dist;
            teleportPos += m_Transform.position;



            _node = A_Star_Pathfinding.Instance.WalkableNodeFromWorldPoint(teleportPos, m_Actor.Bounds, m_Actor.WalkableNodes);


            if (_node == null)
            {
                //teleportPos = A_Star_Pathfinding.Instance.WorldPointFromWorldPoint(teleportPos);
                teleportPos.y = m_Transform.position.y;
                teleportPos.y -= Utilities.CalculateObjectBounds(gameObject, false).y / 2;
            }
            else
            {
                isValid = _node.IsWalkable(m_Actor.WalkableNodes);
                teleportPos = _node.WorldPosition;
            }
           

            if (!isValid)
                continue;



            teleportPos.y += Utilities.CalculateObjectBounds(gameObject, false).y / 2;
            teleportPos.y += 0.01f;




            isValid = Vector3.Distance(teleportPos, target.LastKnownPosition) > minDistanceFromTarget;

            if (!isValid)
                continue;

            //yield return StartCoroutine(TeleportToPosition(newPos));

        } while (!isValid);

        if (m_Actor.ShowDebug)
        {
            Debug.DrawLine(m_Transform.position, teleportPos, Color.yellow, 5f);
        }

        Vector3 toPos = teleportPos - m_Transform.position;
        toPos = toPos.normalized * m_Movement.Speed;

        ActivateSpectral();

        while (target != null && target.SightedTransform.gameObject.activeInHierarchy && Vector3.Distance(m_Transform.position, teleportPos) > 0.01f)
        {
            m_Transform.position = Vector3.MoveTowards(m_Transform.position, teleportPos, m_Movement.Speed * TELEPORT_SPEEDUP * Time.deltaTime);
            m_Movement.RotateTowards(target.LastKnownPosition);
            yield return null;
        }

        DeactivateSpectral();


        EndBehavior(true,true);
    }



    public override void StartBehavior()
    {
        IsActive = true;

        StartCoroutine(TeleportToTarget());   
    }
   
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

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
