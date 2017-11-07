using UnityEngine;
using System.Collections;
using System;

public class ForceAbility : Ability {

    [Space(5)]
    [Header("Force")]
    [Space(5)]

    [Tooltip("Amount of force to apply to objects within range")]
    [SerializeField]
    float forceStrength;

    [Tooltip("Range of force to be applied")]
    [SerializeField]
    float forceRange;

    [Tooltip("Maximum angle of force from forward angle")]
    [SerializeField]
    [Range(0f, 180f)]
    float maxAngle = 45f;
    
    [Tooltip("Falloff of force power/damage over range")]
    [SerializeField]
    AnimationCurve falloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);


    Transform m_Transform;
    UnitController m_Controller;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }

    public override void Initialize(Transform _transform)
    {
        m_Controller = _transform.GetComponent<UnitController>();
    }
    public override void Terminate()
    {
       // throw new NotImplementedException();
    }



    public override void ActivateAbility()
    {
        ApplyForce();

        //ChargeArithmetic(-activationDelta);
        SetCharge(0f);
    }



    public override void DeactivateAbility()
    {
       // throw new NotImplementedException();
    }


    void ApplyForce()
    {
        //Get all colliders within range
        Collider[] hitColls = Physics.OverlapSphere(m_Transform.position, forceRange);

        Rigidbody hitRigid;
        for (int i = 0; i < hitColls.Length; i++)
        {
            hitRigid = hitColls[i].GetComponent<Rigidbody>();

            //Ignore triggers, objects without health, and objects in ignore mask
            if (hitColls[i].isTrigger || hitRigid == null)
                continue;



            Vector3 forceVector = hitColls[i].transform.position - m_Transform.position;
            forceVector.Normalize();

            if (Vector3.Angle(m_Transform.forward, forceVector) > maxAngle)
                continue;




            //Check distance from grenade and apply damage falloff
            float distPercent = Mathf.Clamp01(Vector3.Distance(hitColls[i].transform.position, m_Transform.position) / forceRange);

            if (distPercent > 1f || distPercent < 0)
                continue;


           
            forceVector *= forceStrength * falloffCurve.Evaluate(distPercent);

            hitRigid.AddForce(forceVector, ForceMode.Impulse);
        }
        
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        Utilities.ValidateCurve_Times(falloffCurve, 0f, 1f);
    }
}
