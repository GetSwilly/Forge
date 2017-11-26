using UnityEngine;
using System.Collections;
using System;

public class MeleeBehavior : BaseUtilityBehavior
{
    [Tooltip("Maximum angle of attack")]
    [SerializeField]
    [Range(0f, 180f)]
    protected float attackAngle = 45f;

    [Tooltip("Range of attack")]
    [SerializeField]
    protected float attackRange = 2f;

    [Tooltip("Amount of force applied to each hit object")]
    [SerializeField]
    float attackForce = 2f;

    [Tooltip("Amount of damage applied to each hit object")]
    [SerializeField]
    int attackPower = 0;

    [Tooltip("Falloff of force/power of hit based on percentage of range")]
    [SerializeField]
    AnimationCurve fallOffCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

  

    void Strike()
    {
        Collider[] hitColls = Physics.OverlapSphere(m_Transform.position, attackRange);
        Health hitHealth;

        for (int i = 0; i < hitColls.Length; i++)
        {
            if (hitColls[i].isTrigger)
                continue;

            if (hitColls[i].transform == m_Transform)
                continue;
            

            Vector3 disparityVector = hitColls[i].transform.position - m_Transform.position;
            disparityVector.y = 0;

            if (Vector3.Angle(m_Transform.forward, disparityVector) > attackAngle)
                continue;


            float fallOffVal = fallOffCurve.Evaluate(disparityVector.magnitude / attackRange);


            hitHealth = hitColls[i].GetComponent<Health>();
            if (hitHealth != null)
            {
                hitHealth.HealthArithmetic(-attackPower * fallOffVal, false, m_Transform.parent);
            }

            Rigidbody _rigidbody = hitColls[i].GetComponent<Rigidbody>();
            if (_rigidbody != null)
                _rigidbody.AddForce(disparityVector.normalized * attackForce * fallOffVal, ForceMode.Impulse);

        }

        EndBehavior(true, true);
    }





    public override void StartBehavior()
    {
        base.StartBehavior();

        Strike();
    }



    public override float GetBehaviorScore()
    {

        if (m_Actor.TargetObject == null || Vector3.Distance(m_Actor.TargetObject.LastKnownBasePosition, m_Transform.position) > attackRange)
            return 0f;


        float angle = Vector3.Angle(m_Transform.forward, m_Actor.TargetObject.LastKnownBasePosition);
        float percentage = Mathf.Clamp01(angle / attackAngle);

        return utilityCurve.Evaluate(percentage);
    }



    public override bool CanStartSubBehavior
    {
        get { return false; }
    }



    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }



    protected override void OnValidate()
    {
        base.OnValidate();

        Utilities.ValidateCurve_Times(fallOffCurve, 0f, 1f);
        Utilities.ValidateCurve_Times(utilityCurve, 0f, 1f);
    }


    public override string ToString()
    {
        return "Melee";
    }
}
