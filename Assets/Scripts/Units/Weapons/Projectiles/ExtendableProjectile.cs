using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class ExtendableProjectile : MonoBehaviour, IProjectile {


    [SerializeField]
    [Range(0f,1f)]
    float smoothRate;


    double currentRange;
    double desiredRange;
    double maxRange;

    LayerMask friendlyMask;

    Transform owner;
    int power;


    List<Action<Health>> onImpactMethods = new List<Action<Health>>();

    [SerializeField]
    AudioClip impactSound;

    float totalDist;
    float timer;


    [SerializeField]
    bool canPenetrateObjects = false;
    GameObject nearestObject = null;


    bool isActive = false;

    AttributeModifier m_Modifier;

    Transform m_Transform;
    BoxCollider m_Collider;
   

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Collider = GetComponent<BoxCollider>();

        m_Modifier = GetComponent<AttributeModifier>();
    }
    
    void OnDisable()
    {
        ResetOnImpact();
        StopAllCoroutines();
    }


    void Update()
    {
        if (nearestObject != null)
        {
            if (!nearestObject.activeInHierarchy)
            {
                nearestObject = null;
            }
            else
            {
                Vector3 toVector = nearestObject.transform.position - m_Transform.position;
                toVector.y = 0;

                if (DesiredRange > toVector.magnitude)
                    DesiredRange = toVector.magnitude;
            }
        }

        double diff = DesiredRange - CurrentRange;

        if (diff == 0)
            return;

        //double delta = (diff > 0 ? growthRate : decayRate) * Time.deltaTime;

        //if (Math.Abs(delta) > Math.Abs(diff))
        //    delta = diff;


        //CurrentRange += delta;
        CurrentRange = Mathf.Lerp((float)CurrentRange, (float)MaxRange, smoothRate);


        isActive = CurrentRange > 0;

        Vector3 newSize = m_Collider.size;
        newSize.z = (float)CurrentRange;
        m_Collider.size = newSize;

        m_Collider.center = new Vector3(0, 0, (float)(CurrentRange / 2.0));

        //DesiredRange += decayRate * Time.deltaTime;
    }






    public void Initialize(Transform newOwner, LayerMask newFriendly, Vector3 dir, int newPower, bool _critical, float newRange)
    {
        throw new NotImplementedException();
    }
    public void Initialize(Transform newOwner, LayerMask newFriendly, Vector3 dir, int newPower, bool _critical, float newSpeed, float newRange)
    {
        owner = newOwner;
        friendlyMask = newFriendly;

        power = newPower;
        desiredRange = newRange;
        maxRange = newRange;

        isActive = true;   
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


        DesiredRange = 0;
    }
    public void Disable(Transform hitTransform, Vector3 hitPosition)
    {
        throw new NotImplementedException();
    }









    public Vector3 Position
    {
       get { return m_Transform.position; }
    }
    public Vector3 Direction
    {
        get { return m_Transform.forward; }
    }
    public GameObject GameObject
    {
       get { return gameObject; }
    }




    public bool IsCritical
    {
        get { return false; }
    }

    public LayerMask FriendlyMask
    {
        get { return friendlyMask; }
    }

    public Transform Owner
    {
        get { return owner; }
    }

    public int Power
    {
        get{ return power; }
    }
   
    protected double CurrentRange
    {
        get { return currentRange; }
        set
        {
            currentRange = value;

            if(currentRange < 0.0)
            {
                currentRange = 0;
            }

            if(currentRange > MaxRange)
            {
                currentRange = MaxRange;
            }
        }
    }
    protected double DesiredRange
    {
        get { return desiredRange; }
        set
        {
            desiredRange = value;

            if (desiredRange < 0.0)
            {
                desiredRange = 0;
            }

            if (desiredRange > MaxRange)
            {
                desiredRange = MaxRange;
            }
        }
    }
    public double MaxRange
    {
        get { return maxRange; }
        set
        {
            maxRange = value;
            
            if(maxRange < 0)
            {
                maxRange = 0;
            }

            DesiredRange = DesiredRange;
        }
    }






    void OnTriggerEnter(Collider coll)
    {

        if (!isActive || coll.isTrigger || coll.transform == m_Transform.parent)
            return;



        if (!Utilities.IsInLayerMask(coll.gameObject, friendlyMask))
        {


            if (m_Modifier != null)
                m_Modifier.AddObject(coll);


            Health otherHealth = coll.gameObject.GetComponent<Health>();

            if (otherHealth != null)
            {
                otherHealth.HealthArithmetic(Power, false, owner, Direction);


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


        if (!canPenetrateObjects)
        {
            if (nearestObject == null || !nearestObject.activeInHierarchy || Vector3.Distance(m_Transform.position, nearestObject.transform.position) > Vector3.Distance(m_Transform.position, coll.transform.position))
            {
               
                m_Modifier.RemoveObject(nearestObject);
                nearestObject = coll.gameObject;
                DesiredRange = Vector3.Distance(m_Transform.position, nearestObject.transform.position);
            }
        }
    }
    void OnCollisionStay(Collision coll)
    {
        if (!isActive || !Utilities.IsInLayerMask(coll.gameObject, friendlyMask))
            return;


        if (coll.transform == m_Transform.parent)
            return;
        /*
        Vector3 toVector = coll.transform.position - myTransform.parent.position;

        if (Vector3.Angle(toVector, myTransform.forward) > 90)
            return;
*/



        Health collHealth = coll.gameObject.GetComponent<Health>();

        if (collHealth != null)
            collHealth.HealthArithmetic(-power, false, owner, Direction);

        ActivateOnImpact(collHealth);
    }
    void OnTriggerExit(Collider coll)
    {
        if (m_Modifier != null)
            m_Modifier.RemoveObject(coll.gameObject);

        if (coll.gameObject == nearestObject)
            nearestObject = null;
    }

    



    void OnValidate()
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
}
