﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
[RequireComponent(typeof(MovementController))]
public class Charge : BaseUtilityBehavior
{
    [SerializeField]
    bool isCrit = false;


    [SerializeField]
    float chargeTime = 2f;

    [SerializeField]
    [Range(0.5f, 5f)]
    float chargeSpeedup = 1.5f;
    
    [SerializeField]
    float chargeForce = 2f;
    
    [SerializeField]
    int chargeDamage = 0;

    [SerializeField]
    [Range(0f, 180f)]
    float attackAngle = 5f;

    [SerializeField]
    AudioClip chargeSound;
    
    
    MovementController m_Movement;
    

    public override void Awake()
    {
        base.Awake();
        m_Movement = GetComponent<MovementController>();
    }



    IEnumerator ChargeAttack()
    {
        
        float timer = chargeTime;

     

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            

            m_Movement.Move(m_Transform.forward);


            yield return null;
        }

        EndBehavior(true, true);
    }
    
    

    public override void StartBehavior()
    {
        IsActive = true;

        m_Movement.AddSpeedMultiplier(chargeSpeedup);
        StartCoroutine(ChargeAttack());
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopCoroutine(ChargeAttack());
        m_Movement.RemoveSpeedMultiplier(chargeSpeedup);

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

    public override float GetBehaviorScore()
    {

        if (m_Actor.TargetObject == null)
            return utilityCurve.Evaluate(1f);


        float angle = Vector3.Angle(m_Transform.forward, m_Actor.TargetObject.LastKnownPosition);
        float percentage = Mathf.Clamp01(angle / attackAngle);

        return utilityCurve.Evaluate(percentage);
    }

    

    public override bool CanStartSubBehavior
    {
       get { return false; }
    }

    


    public void OnCollision(Collider coll)
    {
        if (coll.isTrigger)
            return;

        IMemorable mem = coll.GetComponent<IMemorable>();

        if (mem == null)
            return;

        if (!IsActive)
            return;

        Rigidbody collidingRigid = coll.GetComponent<Rigidbody>();

        if (collidingRigid == null)
            return;

        Vector3 toVector = coll.transform.position - m_Transform.position;
        toVector.Normalize();
        toVector *= chargeForce;

        collidingRigid.AddForce(toVector, ForceMode.Impulse);



        if (!m_Actor.IsEnemy(mem.GameObject.GetComponent<ITeamMember>()))
        {

            Health collHealth = coll.gameObject.GetComponent<Health>();

            if (collHealth != null)
            {
                collHealth.HealthArithmetic(-chargeDamage, isCrit, m_Transform);
            }
        }
    }

    


    public override string ToString()
    {
        return "Charge";
    }


}
