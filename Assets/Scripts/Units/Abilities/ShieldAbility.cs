using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Shield))]
public class ShieldAbility : Ability {

	[SerializeField] float activateRate;

    Shield myShield;


    void Awake()
    {
        myShield = GetComponent<Shield>();
    }
    public override void Initialize(Transform _transform)
    {
        throw new NotImplementedException();
    }
    public override void Terminate()
    {
        throw new NotImplementedException();
    }




    public override void ActivateAbility(){
		float temp = currentCharge - (activateRate * Time.deltaTime);

		if(temp < 0){
			DeactivateShield();
			return;
		}

		currentCharge -= activateRate * Time.deltaTime;
		ActivateShield();
	}
    public override void DeactivateAbility()
    {
    }



	void ActivateShield()
    {
		myShield.enabled = true;
	}

	void DeactivateShield()
    {
        myShield.enabled = false;
	}
	

	
	public override bool CanUseAbility()
    {
        return currentCharge > 0f;
	}

    /*
	void OnTriggerEnter(Collider coll){
		IProjectile _projectile = coll.GetComponent<IProjectile>();

		if(_projectile != null && !Utilities.IsInLayerMask(transform.parent.gameObject, _projectile.GetFriendlyMask())){
			coll.gameObject.SetActive(false);
		}
	}*/
}
