using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
public class Grenade : UtilityItem {

    [SerializeField]
    LayerMask ignoreMask;

	[SerializeField] float fuseTime;
	[SerializeField] float radius;
	[SerializeField] int power;
	[SerializeField] [Range(0f,1f)] float criticalChance;
	[SerializeField] [Range(0f,3f)] float criticalMultiplier;

	[SerializeField] bool ignoreObstacles = false;

	[SerializeField] AnimationCurve damageFallOff = AnimationCurve.Linear(0f,1f,1f,1f);
	TrailRenderer m_TrailRenderer;


	public override void Awake()
    {
        base.Awake();

		m_TrailRenderer = GetComponent<TrailRenderer>();
	}

	public override void Activate(Transform _owner, List<Stat> stats)
    {
		owner = _owner;

		StartCoroutine(ResetTrail());

		StartCoroutine(FuseDelay());
	}

	IEnumerator ResetTrail()
    {
		
		float trailTime = m_TrailRenderer.time;
		m_TrailRenderer.time = 0;
		
		yield return new WaitForSeconds(0f);
		
		m_TrailRenderer.time = trailTime;   
	}
	

	IEnumerator FuseDelay()
    {
		yield return new WaitForSeconds(fuseTime);

		Explode();
	}

	void Explode()
    {
		StopAllCoroutines();

		//Get all colliders within range
		Collider[] hitColls = Physics.OverlapSphere(myTransform.position, radius);

		Health hitHealth;
		for(int i = 0; i < hitColls.Length; i++){
			hitHealth = hitColls[i].GetComponent<Health>();

			//Ignore triggers, objects without health, and objects in ignore mask
			if(hitColls[i].isTrigger || hitHealth == null || Utilities.IsInLayerMask(hitColls[i].gameObject.layer, ignoreMask))
				continue;
		
			//If ignoring blocking obstacles
			if(!ignoreObstacles){

				//Check if an object is between the grenade and intended object

				Vector3 toVector = hitColls[i].transform.position - myTransform.position;

				RaycastHit[] rayHits = Physics.RaycastAll(myTransform.position, toVector, toVector.magnitude);
				bool validHit = true;

				for(int j = 0; j < rayHits.Length; j++){
					if(!Utilities.IsInLayerMask(rayHits[j].collider.gameObject.layer, ignoreMask) && rayHits[j].collider != hitColls[i]){
						validHit = false;
						break;
					}
				}

				if(!validHit)
					continue;
			}

			//Check if critical hit
			bool isCrit = Random.value <= criticalChance;
            float dmg = power;
            
            if(isCrit)
                dmg *= criticalMultiplier;

			//Check distance from grenade and apply damage falloff
			float distPercent = Mathf.Clamp01(Vector3.Distance(hitColls[i].transform.position, myTransform.position) / radius);

			if(distPercent > 1f || distPercent < 0)
				continue;

			dmg *= damageFallOff.Evaluate(distPercent);

			hitHealth.HealthArithmetic(-(int)dmg, isCrit, owner);
		}

		Deactivate();
	}


    void Deactivate()
    {
        gameObject.SetActive(false);
    }



    void OnValidate()
    {
        Utilities.ValidateCurve_Times(damageFallOff, 0f, 1f);
    }

	/*
	void OnDrawGizmos(){
		Gizmos.color = Color.yellow;

		Gizmos.DrawWireSphere(transform.position, range);
	}*/
}
