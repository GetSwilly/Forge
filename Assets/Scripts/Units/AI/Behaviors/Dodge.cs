using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MovementController))]
public class Dodge : BaseUtilityBehavior {

    static readonly int MAX_ITERATIONS = 1;
    static readonly float TELEPORT_SPEEDUP = 10f;

    enum MovementType { Physical, Teleport }


    [SerializeField]
    MovementType m_MovementType = MovementType.Physical;


    [Tooltip("Amount of time dodge will be applied for")]
    [SerializeField]
    float dodgeTime;

    [Tooltip("Speed of dodge action")]
    [SerializeField]
    float dodgeSpeed;
    
    
    bool isRecharging = false;
    
    Rigidbody m_Rigidbody;
    List<Collider> m_Colliders;
    Renderer[] m_Renderers;
    MovementController m_Movement;



    public override void Awake()
    {
        base.Awake();
        
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Movement = GetComponent<MovementController>();
        m_Renderers = GetComponents<Renderer>();
    }
    void Start()
    {
        m_Colliders = new List<Collider>();
        Collider[] tempColls = GetComponents<Collider>();


        for (int i = 0; i < tempColls.Length; i++)
        {
            if (tempColls[i].isTrigger)
                continue;

            m_Colliders.Add(tempColls[i]);
        }
    }


    void ActivateSpectral()
    {
        for (int i = 0; i < m_Colliders.Count; i++)
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
        for (int i = 0; i < m_Colliders.Count; i++)
        {
            m_Colliders[i].enabled = true;
        }

        for (int i = 0; i < m_Renderers.Length; i++)
        {
            Color c = m_Renderers[i].material.color;
            c.a = 1f;
            m_Renderers[i].material.color = c;
        }

        m_Rigidbody.isKinematic = false;
    }



    IEnumerator CalculateDodge()
    {
        List<IProjectile> _projectiles = m_Actor.NearbyProjectiles;

        Vector3 bestPosition = m_Transform.position;
        bool isPerfectPosition;

        float maxDistance = dodgeSpeed * dodgeTime;
        Vector3 myBounds = Utilities.CalculateObjectBounds(gameObject, false);

        int iterationNum = 0;
        RaycastHit hit;

        do
        {

            yield return null;


            isPerfectPosition = true;
            iterationNum++;


            Vector3 randomPosition = m_Transform.position + (UnityEngine.Random.onUnitSphere * maxDistance);

            if (A_Star_Pathfinding.Instance != null)
            {
                randomPosition = A_Star_Pathfinding.Instance.NodeFromWorldPoint(randomPosition).WorldPosition;
            }
            else
            {
                randomPosition.y = 100f;

                LayerMask groundMask = LayerMask.NameToLayer("Ground");


                if (Physics.Raycast(new Ray(randomPosition, Vector3.down), out hit, 101f, groundMask))
                {
                    randomPosition.y = hit.point.y + (myBounds.y / 2f);
                }
                else
                {
                    randomPosition.y = m_Transform.position.y;
                }
            }


            for (int i = 0; i < _projectiles.Count; i++)
            {

                if (!isPerfectPosition)
                    break;


                for (int k = 0; k < m_Colliders.Count; k++)
                {
                    if (m_Colliders[k].Raycast(new Ray(_projectiles[i].Position, _projectiles[i].Direction), out hit, Vector3.Distance(_projectiles[i].Position, m_Transform.position)))
                    {
                        isPerfectPosition = false;
                    }
                }

            }


            if (!isPerfectPosition)
                continue;

            bestPosition = randomPosition;

        } while (!isPerfectPosition && iterationNum < MAX_ITERATIONS);




        switch (m_MovementType)
        {
            case MovementType.Physical:
                StartCoroutine(Roll(bestPosition - m_Transform.position));
                break;
            case MovementType.Teleport:
                StartCoroutine(Teleport(bestPosition));
                break;
            default:
                EndBehavior(true, true);
                break;
        }
    }
    IEnumerator Roll(Vector3 dodgeVector)
    {
        dodgeVector = dodgeVector.normalized * dodgeSpeed;
       

        float timer = 0;
        while (timer < dodgeTime)
        {

            timer += Time.deltaTime;

            dodgeVector.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = dodgeVector;

            yield return null;
        }

        EndBehavior(true, true);
    }
    IEnumerator Teleport(Vector3 teleportPosition)
    {
        Node _node = A_Star_Pathfinding.Instance.WalkableNodeFromWorldPoint(teleportPosition, m_Actor.Bounds, m_Actor.WalkableNodes);

        if (m_Actor.ShowDebug)
        {
            Debug.DrawLine(m_Transform.position, teleportPosition, Color.yellow, 5f);
        }
        
        ActivateSpectral();

        while (Vector3.Distance(m_Transform.position, teleportPosition) > 0.01f)
        {
            m_Transform.position = Vector3.MoveTowards(m_Transform.position, teleportPosition, m_Movement.Speed * TELEPORT_SPEEDUP * Time.deltaTime);
             yield return null;
        }

        DeactivateSpectral();


        EndBehavior(true, true);
    }



    public override void StartBehavior()
    {
        IsActive = true;
        StartCoroutine(CalculateDodge());
    }

    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        IsActive = false;

        StopAllCoroutines();
        
        Vector3 newVel = Vector3.zero;
        newVel.y = m_Rigidbody.velocity.y;
        m_Rigidbody.velocity = newVel;
        
        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

    public override float GetBehaviorScore()
    {
        float sightRange = m_Actor.SightRange;

        float shortestDist = float.MaxValue;

       List<IProjectile> _projectiles = m_Actor.NearbyProjectiles;

        if (_projectiles.Count == 0)
            return 0f;

        for(int i = 0; i < _projectiles.Count; i++)
        {
            float dist = Vector3.Distance(_projectiles[i].Position, m_Transform.position);

            if (dist < shortestDist)
                shortestDist = dist;
            
        }

        float pctg = Mathf.Clamp01(shortestDist / sightRange);

        return utilityCurve.Evaluate(pctg);
    }

    public override bool CanStartBehavior
    {
        get { return base.CanStartBehavior && !isRecharging; }
    }

    public override bool CanEndBehavior
    {
        get { return false; }
    }

    public override bool CanStartSubBehavior
    {
       get { return true; }
        /*
        if (subBehavior == null)
            return true;


        return subBehavior.CanEndBehavior() || subBehavior.CanStartBehavior();*/
    }
    

    public override string ToString()
    {
        return "Dodge";
    }

}
