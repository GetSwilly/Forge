using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class StandardProjectile : MonoBehaviour, IProjectile {

    Team m_Team;

    Transform owner;
    Vector3 direction;
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


    Transform m_Transform;
    Rigidbody m_Rigidbody;
    TrailRenderer m_TrailRenderer;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_TrailRenderer = GetComponent<TrailRenderer>();
    }
    void OnEnable()
    {
        previousPosition = m_Transform.position;
        totalDist = 0;
    }


    public void Initialize(Transform newOwner, Team _team, Vector3 dir, int newPower, bool _critical, float newRange)
    {
        throw new NotImplementedException();
    }
    public void Initialize(Transform newOwner, Team _team, Vector3 dir, int newPower, bool _critical, float newSpeed, float newRange)
    {

        if (m_Transform == null)
            m_Transform = GetComponent<Transform>();

        if (m_Rigidbody == null)
            m_Rigidbody = GetComponent<Rigidbody>();

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



        //startPosition = myTransform.position;
        previousPosition = m_Transform.position;
        totalDist = 0;

        m_Team = _team;

        power = newPower;

        isCritical = _critical;

        m_Rigidbody.velocity = dir.normalized * newSpeed;

        maxRange = newRange;
        maxTime = Mathf.Abs((float)maxRange / newSpeed) * 1.5f;
        timer = 0f;


        Validate();

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



        if (hitTransform != null)
        {
            AudioSource _audio = GetComponent<AudioSource>();

            if (_audio != null && impactSound != null)
            {
                _audio.PlayOneShot(impactSound);
            }



            Health hitHealth = hitTransform.GetComponent<Health>();

            if (hitHealth != null)
                ActivateOnImpact(hitHealth);
        }


        gameObject.SetActive(false);
    }
    public void Disable(Transform hitTransform, Vector3 hitPosition)
    {
        throw new NotImplementedException();
    }





    void Update()
    {
        timer += Time.deltaTime;

        totalDist += Vector3.Distance(previousPosition, m_Transform.position);
        previousPosition = m_Transform.position;

        if (timer > maxTime || totalDist > maxRange)
        {
            m_TrailRenderer.enabled = false;
            gameObject.SetActive(false);
        }

    }



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
        get { return m_Rigidbody.velocity; }
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




    void OnCollisionEnter(Collision coll)
    {

        if (coll.collider.isTrigger)
            return;

        ITeamMember teamMember = coll.gameObject.GetComponent<ITeamMember>();

        if (!(teamMember != null && TeamUtility.IsFriendly(Team.FriendlyTeams, teamMember.GetCurrentTeam())))
        {
            Health otherHealth = coll.gameObject.GetComponent<Health>();

            if (otherHealth != null)
            {
                otherHealth.HealthArithmetic(Power, IsCritical, owner, m_Rigidbody.velocity.normalized);


                /*
				AudioSource _audio = GetComponent<AudioSource>();

				if(_audio != null && impactSound != null){
					_audio.PlayOneShot(impactSound);
				}
				
				if(OnImpact != null)
					OnImpact(otherHealth);
                */


                Disable(coll.gameObject.GetComponent<Transform>());
            }

        }

        Disable();
    }




    void OnDisable()
    {
        ResetOnImpact();

        //Debug.Log("Projectile disabled");
    }

    void Validate()
    {
        MaxRange = MaxRange;
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
        for(int i = 0;i < onImpactMethods.Count; i++)
        {
            if(onImpactMethods[i] == alertMethod)
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
}
