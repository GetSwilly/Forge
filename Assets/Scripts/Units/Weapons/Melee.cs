using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Melee : Weapon {

    [SerializeField]
    [Range(0, 180)]
    protected int attackAngle = 45;

    [SerializeField]
    float attackForce = 0f;


    private List<HitDetector> m_Detectors = new List<HitDetector>();



    public override void Awake()
    {
        base.Awake();


        HitDetector[] _detectors = GetComponentsInChildren<HitDetector>();
        for(int i = 0; i < _detectors.Length; i++)
        {
            _detectors[i].OnHit += Hit;
            m_Detectors.Add(_detectors[i]);
        }
    }
    

    private void EnableDetectors()
    {
        for (int i = 0; i < m_Detectors.Count; i++)
        {
            m_Detectors[i].enabled = true;
        }
    }
    private void DisableDetectors()
    {
        for(int i = 0; i < m_Detectors.Count; i++)
        {
            m_Detectors[i].enabled = false;
        }
    }

    private void Hit(Collider coll)
    {
        throw new NotImplementedException();
    }



    public override void ActivatePrimary()
    {
		DisableEffects();

        EnableDetectors();


        //TODO -- Play Animation, wait for animation to finish, disable detectors

        DisableDetectors();
		

		attackTimerPrimary = AttackRatePrimary;
	}

    //public override void ActivatePrimary()
    //{

    //    DisableEffects();

    //    bool isCrit = IsCritical();
    //    int hitPower = isCrit ? (int)(AttackPower * CriticalMultiplier) : AttackPower;

    //    //EnableEffects(isCrit);

    //    Collider[] hitColls = Physics.OverlapSphere(myTransform.position, AttackRange);
    //    Health hitHealth;

    //    for (int i = 0; i < hitColls.Length; i++)
    //    {


    //        if (hitColls[i].isTrigger)
    //            continue;

    //        if (hitColls[i].gameObject == gameObject || hitColls[i].transform == myOwner)
    //            continue;

    //        // if(Utilities.IsInLayerMask(hitColls[i].gameObject, friendlyMask))
    //        //   continue;







    //        Vector3 disparityVector = hitColls[i].transform.position - myTransform.position;
    //        disparityVector.y = 0;

    //        if (Vector3.Angle(myTransform.forward, disparityVector) > attackAngle)
    //            continue;


    //        hitHealth = hitColls[i].GetComponent<Health>();

    //        if (hitHealth != null)
    //        {
    //            hitHealth.HealthArithmetic(-hitPower, isCrit, myTransform.parent);

    //            AlertWeaponCasualty(hitHealth);
    //        }

    //        Rigidbody _rigidbody = hitColls[i].GetComponent<Rigidbody>();

    //        if (_rigidbody != null)
    //            _rigidbody.AddForce(disparityVector.normalized * attackForce, ForceMode.Impulse);

    //    }

    //    attackTimerPrimary = AttackRatePrimary;
    //}


    public override void ActivateSecondary()
    {

	}

    public override void ActivateTertiary() { }


    public override void EnableEffects() { }

    public override void DisableEffects() { }



	public override float GetPercentage()
    {
        float temp = Mathf.Clamp(attackTimerPrimary, 0f, AttackRatePrimary);
        temp /= AttackRatePrimary;

		return 1 - temp;
	}




    public override void DeactivatePrimary() { }

    public override void DeactivateSecondary() { }

    public override bool CanActivateSecondary()
    {
        return true;
    }

    public override void DeactivateTertiary() { }

    public override bool CanActivateTertiary()
    {
        return true;
    }
}
