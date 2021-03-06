﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using mattatz.VoxelSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(Voxelizer))]
public class StandardProjectile : MonoBehaviour, IProjectile
{
    [SerializeField]
    LayerMask collisionLayer;

    Team m_Team;

    Transform owner;
    Vector3 direction;
    float speed;
    double maxRange = 10;
    float maxTime;
    int power;
    bool isCritical;

    List<Action<Health>> onImpactMethods = new List<Action<Health>>();

    [SerializeField]
    AudioClip impactSound;

    Vector3 previousPosition;
    float totalDist;
    float timer;

    bool isActive;
    Transform m_Transform;
    TrailRenderer m_TrailRenderer;
    Voxelizer m_Voxelizer;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_TrailRenderer = GetComponent<TrailRenderer>();
        m_Voxelizer = GetComponent<Voxelizer>();
    }
    void OnEnable()
    {
        previousPosition = m_Transform.position;
        totalDist = 0;
    }
    void OnDisable()
    {
        ResetOnImpact();
    }


    void FixedUpdate()
    {
        if (!isActive)
            return;

        timer += Time.deltaTime;

        previousPosition = m_Transform.position;

        float movementAmount = Speed * Time.deltaTime;
        m_Transform.Translate(Direction * movementAmount, Space.World);

        totalDist += movementAmount;

        if (timer > maxTime || totalDist > maxRange)
        {
            m_TrailRenderer.enabled = false;
            gameObject.SetActive(false);
        }
        else
        {
            RaycastHit[] hits = Physics.RaycastAll(new Ray(previousPosition, (m_Transform.position - previousPosition).normalized), (m_Transform.position - previousPosition).magnitude, collisionLayer);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.isTrigger)
                    continue;

                ProcessImpact(hits[i]);
            }
        }
    }



    public void Initialize(Transform newOwner, Team _team, Vector3 dir, int newPower, bool _critical, float newRange)
    {
        throw new NotImplementedException();
    }
    public void Initialize(Transform newOwner, Team _team, Vector3 dir, int newPower, bool _critical, float newSpeed, float newRange)
    {

        if (m_Transform == null)
            m_Transform = GetComponent<Transform>();

        owner = newOwner;

        if (owner != null)
        {
            SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer ownerRenderer = owner.GetComponent<SpriteRenderer>();

            if (myRenderer != null && ownerRenderer != null)
            {
                myRenderer.color = ownerRenderer.color;
            }
        }


        StartCoroutine(ResetTrail());


        previousPosition = m_Transform.position;
        totalDist = 0;

        m_Team = _team;

        power = newPower;

        isCritical = _critical;

        //m_Rigidbody.velocity = dir.normalized * newSpeed;
        Direction = dir.normalized;

        Speed = newSpeed;

        maxRange = newRange;
        maxTime = Mathf.Abs((float)maxRange / newSpeed) * 1.5f;
        timer = 0f;

        isActive = true;

        OnValidate();

        m_TrailRenderer.enabled = true;

    }
    IEnumerator ResetTrail()
    {

        float trailTime = m_TrailRenderer.time;
        m_TrailRenderer.time = 0;

        yield return new WaitForSeconds(0f);

        m_TrailRenderer.time = trailTime;
    }


    public void Disable()
    {
        Disable(null);
    }
    public void Disable(Transform hitTransform)
    {
        isActive = false;

        if (hitTransform != null)
        {
            AudioSource _audio = GetComponent<AudioSource>();

            if (_audio != null && impactSound != null)
            {
                _audio.PlayOneShot(impactSound);
            }


            Health hitHealth = hitTransform.GetComponent<Health>();

            if (hitHealth != null)
            {
                ActivateOnImpact(hitHealth);
            }

        }

        if (m_Voxelizer != null)
        {
            m_Voxelizer.Activate();
        }

        gameObject.SetActive(false);
    }
    public void Disable(Transform hitTransform, Vector3 hitPosition)
    {
        throw new NotImplementedException();
    }


    void ProcessImpact(RaycastHit hit)
    {
        ProcessImpact(hit.collider, hit.point);
    }
    void ProcessImpact(Collider coll, Vector3 impactPoint)
    {
        if (coll.isTrigger)
            return;

        if (impactPoint != null)
        {
            m_Transform.position = impactPoint;
        }

        Team teamMember = coll.gameObject.GetComponent<Team>();

        if (!(teamMember != null && m_Team.IsFriendly(teamMember)))
        {
            Health otherHealth = coll.gameObject.GetComponent<Health>();

            if (otherHealth != null)
            {
                otherHealth.HealthArithmetic(Power, IsCritical, owner, Direction);


                /*
				AudioSource _audio = GetComponent<AudioSource>();

				if(_audio != null && impactSound != null){
					_audio.PlayOneShot(impactSound);
				}
				
				if(OnImpact != null)
					OnImpact(otherHealth);
                */


                Disable(coll.gameObject.transform);
                return;
            }

        }

        Disable();
    }





    void ActivateOnImpact(Health _health)
    {
        for (int i = 0; i < onImpactMethods.Count; i++)
        {
            onImpactMethods[i](_health);
        }
    }
    public void SubscribeToOnImpact(Action<Health> alertMethod)
    {
        //OnImpact = alertMethod;

        if (!onImpactMethods.Contains(alertMethod))
            onImpactMethods.Add(alertMethod);
    }
    public void UnSubscribeToOnImpact(Action<Health> alertMethod)
    {
        for (int i = 0; i < onImpactMethods.Count; i++)
        {
            if (onImpactMethods[i] == alertMethod)
            {
                onImpactMethods.RemoveAt(i);
                return;
            }
        }

    }
    public void ResetOnImpact()
    {
        onImpactMethods.Clear();
    }


    #region Accessors

    public bool IsCritical
    {
        get { return isCritical; }
    }

    public Transform Owner
    {
        get { return owner; }
    }
    public Team Team
    {
        get { return m_Team; }
    }
    public int Power
    {
        get { return power; }
    }
    public Vector3 Position
    {
        get { return m_Transform.position; }
    }
    public Vector3 Direction
    {
        get { return direction; }// m_Rigidbody.velocity; }
        private set { direction = value; }
    }
    public float Speed
    {
        get { return speed; }
        private set { speed = value; }
    }
    public GameObject GameObject
    {
        get { return gameObject; }
    }
    public double MaxRange
    {
        get { return maxRange; }
        set
        {
            maxRange = value;


            if (maxRange < 0)
            {
                maxRange = 0;
            }
        }

    }

    #endregion

    void OnValidate()
    {
        MaxRange = MaxRange;
    }
}
