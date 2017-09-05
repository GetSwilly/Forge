using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AmmoSystem))]
public class Gun : Weapon {

    static readonly float MINIMUM_FIRE_RATE = 0.1f;
	static readonly float MUZZLE_FLASH_TIME = 0.1f;
    
   

    [Space(15)]
    [Header("Shots")]
    [Space(5)]

    [SerializeField]
    ShotContainer primaryShot;

    [SerializeField]
    ShotContainer secondaryShot;


    ObjectPooler primaryPool, secondaryPool;
    GameObject primaryProjector, secondaryProjector;
    IProjectile primaryProjectorScript, secondaryProjectorScript;

    bool lastUsedIsPrimary = true;


    [Space(15)]
    [Header("Misc.")]
    [Space(5)]

    [SerializeField]
    Transform gunBarrelTransform;


    [SerializeField]
    float shotSpeed = 25f;
    [SerializeField]
    float reloadTime = 1f;
    

    bool isFiring = false;
	bool isReloading = false;
	float curReloadTime = 0f;

	protected GameObject curMuzzleFlash;
   

    [SerializeField]
    protected List<WeightedObjectOfGameObject> muzzleFlashes = new List<WeightedObjectOfGameObject>();


    List<GameObject> bulletParents = new List<GameObject>();


    

    [Space(15)]
    [Header("Attack Deterioration")]
    [Space(5)]


    //[SerializeField]
    //float deteriorationOnFire = 0f;

    [SerializeField]
    float passiveDeteriorationRate = 0f;

    float currentDeterioration = 0f;


    [SerializeField]
    AnimationCurve accuracyCurve = AnimationCurve.Linear(0f, 1f, 100f, 1f);

    [SerializeField]
    AnimationCurve pitchCurve = AnimationCurve.Linear(0f, 1f, 100f, 1f);

    [SerializeField]
    AnimationCurve powerCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);

    [SerializeField]
    AnimationCurve rangeCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);

    [SerializeField]
    AnimationCurve rateCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);

    [SerializeField]
    AnimationCurve shotSpeedCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);

    [SerializeField]
    AnimationCurve criticalChanceCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);

    [SerializeField]
    AnimationCurve criticalMultiplierCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);

    

    void Start()
    {
        InitializeProjectors();
    }

    void InitializeProjectors()
    {
        if (primaryShot.Type == ShotContainer.ShotType.Projector && primaryShot.ProjectorObject != null)
        {
            primaryProjector = GameObject.Instantiate(primaryShot.ProjectorObject);
            primaryProjector.transform.SetParent(m_Transform);
            primaryProjector.transform.position = GunBarrelPosition;
            primaryProjector.transform.localRotation = Quaternion.identity;

            primaryProjectorScript = primaryProjector.GetComponent<IProjectile>();
            primaryProjectorScript.SubscribeToOnImpact(AlertWeaponCasualty);
        }

        if (secondaryShot.Type == ShotContainer.ShotType.Projector && secondaryShot.ProjectorObject != null)
        {
            secondaryProjector = GameObject.Instantiate(secondaryShot.ProjectorObject);
            secondaryProjector.transform.SetParent(m_Transform);
            secondaryProjector.transform.position = GunBarrelPosition;
            secondaryProjector.transform.localRotation = Quaternion.identity;

            secondaryProjectorScript = secondaryProjector.GetComponent<IProjectile>();
            secondaryProjectorScript.SubscribeToOnImpact(AlertWeaponCasualty);
        }
    }
    void InitializeObjectPools()
    {
        GameObject p;
        GameObject poolsObj = GameObject.Find("Pools");


        if (primaryShot.Type == ShotContainer.ShotType.Projectile)
        {
            if (primaryPool == null)
                primaryPool = gameObject.AddComponent<ObjectPooler>();


            p = new GameObject(m_Transform.parent.name + " Projectile Pooler -- Primary");


            if (poolsObj != null)
            {
                p.transform.parent = poolsObj.transform;
            }

            primaryPool.Parent = p.transform;
            primaryPool.PooledObject = primaryShot.ProjectileObject;
            primaryPool.Initialize();
            bulletParents.Add(p);

            if (primaryShot.Ammo != null)
                primaryShot.Ammo.OnAmmoChanged += UpdateAmmo;
        }

        if (secondaryShot.Type == ShotContainer.ShotType.Projectile)
        {
            if (secondaryPool == null)
                secondaryPool = gameObject.AddComponent<ObjectPooler>();


            p = new GameObject(m_Transform.parent.name + " Bullet Pooler -- Secondary");
            bulletParents.Add(p);


            if (poolsObj != null)
            {
                p.transform.parent = poolsObj.transform;
            }

            secondaryPool.Parent = p.transform;
            secondaryPool.PooledObject = secondaryShot.ProjectileObject;
            secondaryPool.Initialize();
        }
    }

    public override void UpdateStat(Stat _stat) { }

    public override void Update()
    {
        base.Update();


        //if (!isFiring)
       // {
            CurrentDeterioration += PassiveDeteriorationRate * Time.deltaTime * (GetStatValue(StatType.Speed) + 1);
       // }
    }

    void UpdateProjectors()
    {
        UpdateProjector(primaryShot, primaryProjectorScript);
        UpdateProjector(secondaryShot, secondaryProjectorScript);
    }
    void UpdateProjector(ShotContainer _container, IProjectile _script)
    {
        if (_container == null || _script == null)
            return;


        if (_container.Type != ShotContainer.ShotType.Projector)
            return;

        _script.MaxRange = AttackRange;
    }



    public override void Initialize(Transform _owner, List<Stat> _stats)
    {
        base.Initialize(_owner, _stats);

        DestroyBulletObjects();
        isReloading = false;
        
        InitializeObjectPools();
        
         CurrentDeterioration = 0f;
    }
	public override void Terminate()
    {

        base.Terminate();


		DestroyBulletObjects();

        primaryShot.Ammo.OnAmmoChanged -= UpdateAmmo;
        //secondaryAmmo.OnAmmoChanged -= UpdateAmmo;

        StopAllCoroutines();
	}

	void DestroyBulletObjects()
    {

        for (int i = 0; i < bulletParents.Count; i++)
        {
            Destroy(bulletParents[i]);
        }

        bulletParents = new List<GameObject>();
	}









    Vector3? RaycastSweep(Vector3 aimDir)
    {
        //transform.rotation = Quaternion.Euler(0, 0, 0);
        Vector3 startPos = m_Transform.position;
        Vector3 targetPos = Vector3.zero;
       // Transform targetTransform = null;

        float startAngle = -maxSlope;
        float endAngle = maxSlope;


        // the gap between each ray (increment)
        float incrementAmount = (maxSlope * 2) / NUMBER_RAYSWEEP_INCREMENTS;
        
        RaycastHit targetHit = new RaycastHit();
        float targetDistance = 0;
        //Quaternion targetRotation;

        Vector3 perpendicularVector = new Vector3(-aimDir.z, aimDir.y, aimDir.x);




        for (float i = startAngle; i < endAngle; i += incrementAmount) 
         {
            targetPos = GunBarrelPosition + (Quaternion.AngleAxis(i, perpendicularVector) * aimDir * AttackRange);  //Quaternion.(Quaternion.Euler(i, 0, 0) * aimDir).normalized * AttackRange;
            Ray _sweepRay = new Ray(startPos, targetPos - startPos);


            bool isHit = true;

            
            float nearestTargetDistance = float.MaxValue;
            RaycastHit tempHit = new RaycastHit();

            //Raycast to check for objects in targetMask
            RaycastHit[] hits = Physics.RaycastAll(_sweepRay, AttackRange, targetMask);   //startPos, targetPos - startPos, AttackRange, targetMask);//, out hit, targetMask)
           

            //Ignore colliders that are triggers and choose nearest hit
            for(int k = 0; k < hits.Length; k++)
            {
                if (hits[k].collider.isTrigger)
                    continue;


                float val;
                if ((val = Vector3.Distance(hits[k].point, GunBarrelPosition)) < nearestTargetDistance)
                {
                    nearestTargetDistance = val;
                    tempHit = hits[k];
                }
            }


            isHit = tempHit.collider != null;


            //Ignore colliders that are triggers and check for hit in environment. Invalid hit if environment is hit
            if (isHit)
            {
                hits = Physics.RaycastAll(_sweepRay, Vector3.Distance(tempHit.point, GunBarrelPosition), environmentMask);

                for (int k = 0; k < hits.Length; k++)
                {
                    if (!hits[k].collider.isTrigger)
                    {
                        isHit = false;
                        break;
                    }
                }
            }


            //If hit, check if closest hit
            if (isHit)
            {
                Vector3 _vector = tempHit.point - GunBarrelPosition;

                if (targetHit.collider == null || _vector.magnitude < targetDistance)
                {
                    targetDistance = _vector.magnitude;
                    targetHit = tempHit;

                    //Debug.Log("Hit " + hit.collider.gameObject.name);
                }

                if (showDebug)
                {
                    Debug.DrawLine(startPos, tempHit.point, Color.green, DEBUG_DRAW_TIME);
                }
            }
            else if (showDebug)
            {
                Debug.DrawLine(startPos, targetPos, Color.red, DEBUG_DRAW_TIME);
            }



            /*
            // linecast between points
            if (Physics.Linecast(startPos, targetPos, out hit, targetMask) &&  !Physics.Linecast(startPos,hit.point, environmentMask))
            {
                Vector3 toVector = hit.point - myTransform.position;


                if (targetHit.collider == null || toVector.magnitude < targetDistance)
                {
                    //targetRotation = Quaternion.Euler(i, 0, 0);
                    targetDistance = toVector.magnitude;
                    targetHit = hit;

                    //Debug.Log("Hit " + hit.collider.gameObject.name);
                }

                if (showDebug)
                {
                    Debug.DrawLine(startPos, hit.point, Color.green, DEBUG_DRAW_TIME);
                }
            }
            else if(showDebug)
            {
                Debug.DrawLine(startPos, targetPos, Color.red, DEBUG_DRAW_TIME);
            }
            */
        }


        if (showDebug && targetHit.collider != null)
        {
            Debug.DrawLine(startPos, targetHit.point, Color.yellow, DEBUG_DRAW_TIME);
        }
        //transform.rotation = targetRotation;

        return targetHit.collider == null ? null : (Vector3?)targetHit.point;
    }






    IEnumerator FireProjectile(ShotContainer _container, ObjectPooler _pool, bool isPrimary)
    {
        isFiring = true;



        PlaySound(_container.Sound);


        m_Movement.AddSpeedMultiplier(isPrimary ? primarySpeedSpeedup : secondarySpeedSpeedup);
        m_Movement.AddRotationMultiplier(isPrimary ? primaryRotationSpeedup : secondaryRotationSpeedup);

        List<Shot> _shots = _container.ProjectileShots;
        for (int i = 0; i < _shots.Count; i++)
        {
            if (_shots[i].Delay > 0)
            {
                yield return new WaitForSeconds(_shots[i].Delay);
            }


            float error = 1f - Accuracy;

            if (error != 0f)
                error = UnityEngine.Random.Range(0f, error);

            Vector3 shotDir = m_Transform.TransformDirection(_shots[i].LocalDirection);

            Vector3 shotError = new Vector3(-shotDir.z, 0, shotDir.x);
            shotError *= UnityEngine.Random.value <= 0.5f ? 1f : -1f;  // UnityEngine.Random.Range(-error, error);

            Vector3 fireDir = (shotDir.normalized * (1f - error)) + (shotError * error);  //(shotError * UnityEngine.Random.Range(-error,error));
            fireDir.Normalize();

            Vector3? sweepTarget = RaycastSweep(fireDir);
            if (sweepTarget != null)
            {
                fireDir = ((Vector3)sweepTarget - GunBarrelPosition).normalized;
            }

            GameObject bullet = _pool.GetPooledObject();
            bullet.transform.position = GunBarrelPosition;// +myTransform.TransformPoint(_shot[i].LocalOffset);
            bullet.transform.rotation = m_Transform.rotation;


            IProjectile bScript = bullet.GetComponent<IProjectile>();

            if (bScript != null)
            {
                bullet.SetActive(true);

                bool isCrit = IsCritical();

                float _power = isCrit ? (int)(AttackPower * CriticalMultiplier) : AttackPower;
                _power *= _shots[i].PowerModifier;

                float _speed = ShotSpeed * _shots[i].SpeedModifier;

                float _range = AttackRange * _shots[i].RangeModifier;

                bScript.Initialize(m_Transform.parent, friendlyMask, fireDir, (int)-_power, isCrit, _speed, _range);
                bScript.SubscribeToOnImpact(AlertWeaponCasualty);

                EnableEffects();


                PlaySound(_container.Sound);


                if (isCrit)
                {
                    //	PlaySound(criticalShotSound);
                }

                _container.Ammo.UseAmmo(_shots[i].AmmoCost);

            }

        }


        m_Movement.RemoveSpeedMultiplier(isPrimary ? primarySpeedSpeedup : secondarySpeedSpeedup);
        m_Movement.RemoveRotationMultiplier(isPrimary ? primaryRotationSpeedup : secondaryRotationSpeedup);
        isFiring = false;



        if (isPrimary)
        {
            attackTimerPrimary = AttackRatePrimary;
        }
        else
        {
            attackTimerSecondary = AttackRateSecondary;
        }



        CurrentDeterioration += _container.DeteriorationEffect;
    }
    IEnumerator FireProjector(ShotContainer _shot, IProjectile _script, bool isPrimary)
    {
        if (_shot != null && _script != null)
        {
            isFiring = true;

            if (_shot.ProjectorShot.Delay > 0)
            {
                yield return new WaitForSeconds(_shot.ProjectorShot.Delay);
            }

            float error = 1f - Accuracy;

            if (error != 0f)
                error = UnityEngine.Random.Range(0f, error);

            Vector3 localDir = m_Transform.TransformDirection(_shot.ProjectorShot.LocalDirection);

            Vector3 shotError = new Vector3(-localDir.z, 0, localDir.x);
            shotError *= UnityEngine.Random.value <= 0.5f ? 1f : -1f;

            Vector3 fireDir = (localDir.normalized * (1f - error)) + (shotError * error);
            fireDir.Normalize();


            _script.GameObject.SetActive(true);

            bool isCrit = IsCritical();

            float _power = isCrit ? (int)(AttackPower * CriticalMultiplier) : AttackPower;
            _power *= _shot.ProjectorShot.PowerModifier;

            float _speed = ShotSpeed * _shot.ProjectorShot.SpeedModifier;

            float _range = AttackRange * _shot.ProjectorShot.RangeModifier;

            _script.Initialize(m_Transform.parent, friendlyMask, fireDir, (int)-_power, isCrit, _speed, _range);


            EnableEffects();


            if (isCrit)
            {
                //	PlaySound(criticalShotSound);
            }

            float _cost = _shot.ProjectorShot.AmmoCost * Time.deltaTime;
            _shot.Ammo.UseAmmo(_cost);







            isFiring = false;

            if (isPrimary)
            {
                attackTimerPrimary = AttackRatePrimary;
            }
            else
            {
                attackTimerSecondary = AttackRateSecondary;
            }



            //ChangeAccuracy(-accuracyDeteriorationRate);

            CurrentDeterioration += _shot.DeteriorationEffect * Time.deltaTime;
        }
    }





    public override void ActivatePrimary()
    {
        lastUsedIsPrimary = true;
        

        if (primaryShot.CanShoot())
        {
            DisableEffects();

            if (isReloading)
            {
                StopAllCoroutines();
                isReloading = false;
                AlertWeaponChange(GetPercentage(), true);
            }

            //myAudio.pitch = pitchCurve.Evaluate(currentDeterioration);
            switch (primaryShot.Type)
            {
                case ShotContainer.ShotType.Projectile:
                    StartCoroutine(FireProjectile(primaryShot, primaryPool, true));
                    break;
                case ShotContainer.ShotType.Projector:
                    StartCoroutine(FireProjector(primaryShot, primaryProjectorScript,true));
                    break;
            }
         

            AlertPrimaryActivation();

        }
        else
        {
            ReloadPrimary();
        }
	}
    public override bool CanActivatePrimary()
    {
        return base.CanActivatePrimary() && !isFiring;
    }


    public override void ActivateSecondary()
    {
        lastUsedIsPrimary = false;


        if (secondaryShot.CanShoot())
        {
            DisableEffects();

            if (isReloading)
            {
                StopAllCoroutines();
                isReloading = false;
                AlertWeaponChange(GetPercentage(), true);
            }

            //myAudio.pitch = pitchCurve.Evaluate(currentDeterioration);
            switch (secondaryShot.Type)
            {
                case ShotContainer.ShotType.Projectile:
                    StartCoroutine(FireProjectile(secondaryShot, secondaryPool, false));
                    break;
                case ShotContainer.ShotType.Projector:
                    StartCoroutine(FireProjector(secondaryShot, secondaryProjectorScript, false));
                    break;
            }


            AlertSecondaryActivation();

        }
        else
        {
            ReloadSecondary();
        }
    }
    public override bool CanActivateSecondary()
    {
        return base.CanActivateSecondary() && !isFiring;
    }


    public override void ActivateTertiary()
    {
        if (lastUsedIsPrimary)
        {
            ReloadPrimary();
        }
        else
        {
            ReloadSecondary();
        }
        
    }


    public void ReloadPrimary()
    {
        Reload(primaryShot);
    }
    public void ReloadSecondary()
    {
        Reload(secondaryShot);
    }


    void Reload(ShotContainer _container)
    {
        AmmoSystem _ammo = primaryShot.Ammo;

        if (_ammo.CanReload() && !isReloading)
        {
            StartCoroutine(ReloadWait(_ammo));

            AlertUtilityActivation();
        }
    }

    IEnumerator ReloadWait(AmmoSystem a)
    {
		isReloading = true;
		curReloadTime = 0f;

		AlertWeaponChange(0f, true);
		
		while(curReloadTime < reloadTime)
        {
			curReloadTime += Time.deltaTime;


			AlertWeaponChange(GetPercentage(), false);

			yield return null;
		}
		
		curReloadTime = reloadTime;
	
		if(a.CanReload())
        {
			a.Reload();
		}

		isReloading = false;
	}



	public override void EnableEffects()
    {
        bool isCriticalHit = false;

		SoundClip _sound;//isCriticalHit ? criticalShotSound : normalShotSound;   //Utilities.WeightedSelection( shotSounds.ToArray(), shotSoundProbabilities.ToArray());

		if(isCriticalHit)
        {
			_sound = Utilities.WeightedSelection(criticalAttackSounds.ToArray(), 0f);
		}
        else
        {
            _sound = Utilities.WeightedSelection(primaryActionSounds.ToArray(), 0f);
		}
        
		//if(_sound.Sound != null)
  //      {
		//	myAudio.Stop();

  //          myAudio.volume = _sound.Volume;
  //          myAudio.pitch = _sound.Pitch;
		//	myAudio.PlayOneShot(_sound.Sound);
		//}

		curMuzzleFlash = Utilities.WeightedSelection( muzzleFlashes.ToArray(), 0f);

		if(curMuzzleFlash != null)
        {
			curMuzzleFlash.transform.position = GunBarrelPosition;
			curMuzzleFlash.transform.rotation = m_Transform.parent.rotation;

			curMuzzleFlash.SetActive(true);
		}

		StartCoroutine(DelayDisableEffects());
	}
	public override void DisableEffects()
    {
		//myAudio.Stop();

		if(curMuzzleFlash != null){
			curMuzzleFlash.SetActive(false);
		}
	}
	IEnumerator DelayDisableEffects()
    {

		yield return new WaitForSeconds(MUZZLE_FLASH_TIME);

		DisableEffects();
	}


	public override float GetPercentage()
    {
        if (isReloading)
        {
            return curReloadTime / reloadTime;
        }


        return primaryShot.Ammo == null ? 0f : primaryShot.Ammo.GetAmmoPercentage();
	}

	/*
	void OnDestroy(){
		Destroy(bulletPooler.Parent.gameObject);
		Destroy(this.gameObject);
	}*/


	void UpdateAmmo(float _percent)
    {
		AlertWeaponChange(_percent, false);
	}




    void DeactivateProjector(ShotContainer _container, IProjectile _script)
    {
        if (_container == null || _script == null)
            return;

        if (_container.Type != ShotContainer.ShotType.Projector)
            return;

        //Debug.Log("Deactivating Projector");
        //_script.Disable();
    }

    public override void DeactivatePrimary()
    {
        DeactivateProjector(primaryShot, primaryProjectorScript);
    }
    public override void DeactivateSecondary()
    {
        DeactivateProjector(secondaryShot, secondaryProjectorScript);
    }

    public override void DeactivateTertiary() { }

    public override bool CanActivateTertiary()
    {
        return primaryShot.Ammo != null && primaryShot.Ammo.GetAmmoPercentage() < 100f;
    }



    #region Accessors


    public override int AttackPower
    {
        get { return (int)(base.AttackPower + powerCurve.Evaluate(CurrentDeterioration)); }
    }
    public override float AttackRange
    {
        get { return base.AttackRange + rangeCurve.Evaluate(CurrentDeterioration); }
    }
    public override float AttackRatePrimary
    {
        get {
            float val = base.AttackRatePrimary + rateCurve.Evaluate(CurrentDeterioration);
            return val > MINIMUM_FIRE_RATE ? val : MINIMUM_FIRE_RATE;
        }
    }
    public override float AttackRateSecondary
    {
        get
        {
            float val = base.AttackRateSecondary + rateCurve.Evaluate(CurrentDeterioration);
            return val > MINIMUM_FIRE_RATE ? val : MINIMUM_FIRE_RATE;
        }
    }


    public override float CriticalChance
    {
        get { return base.CriticalChance + criticalChanceCurve.Evaluate(CurrentDeterioration); }
    }
    public override float CriticalMultiplier
    {
        get { return base.CriticalMultiplier + criticalMultiplierCurve.Evaluate(CurrentDeterioration); }
    }


    public float Accuracy
    {
        get { return accuracyCurve.Evaluate(CurrentDeterioration); }
    }
    public float ShotSpeed
    {
        get { return shotSpeed + shotSpeedCurve.Evaluate(CurrentDeterioration); }
        protected set
        {
            shotSpeed = value;

            if (shotSpeed < 0)
            {
                shotSpeed = 0f;
            }
        }
    }



    //protected float DeteriorationOnFire
    //{
    //    get { return deteriorationOnFire; }
    //    set { deteriorationOnFire = value; }
    //}
    protected float PassiveDeteriorationRate
    {
        get { return passiveDeteriorationRate; }
        set { passiveDeteriorationRate = value; }
    }
    protected float CurrentDeterioration
    {
        get { return currentDeterioration; }
        set
        {
            currentDeterioration = Mathf.Clamp(value, 0f, 100f);
            
            UpdateProjectors();
        }
    }



    public float ReloadTime {
		get { return reloadTime; }
		set
        {
            reloadTime = value;

            if(reloadTime <= 0f)
            {
                reloadTime = 0.001f;
            }
        }
	}
	public bool IsReloading{
		get { return isReloading; }
	}
    


    protected Vector3 GunBarrelPosition
    {
        get { return gunBarrelTransform == null ? m_Transform.position : gunBarrelTransform.position; }
    }
	#endregion

    
    void OnValidate()
    {
        ShotSpeed = ShotSpeed;
        
        Utilities.ValidateCurve_Times(accuracyCurve, 0f, 100f);
        Utilities.ValidateCurve_Times(pitchCurve, 0f, 1f);
        Utilities.ValidateCurve_Times(powerCurve, 0f, 100f);
        Utilities.ValidateCurve_Times(rangeCurve, 0f, 100f);
        Utilities.ValidateCurve_Times(rateCurve, 0f, 100f);
        Utilities.ValidateCurve_Times(shotSpeedCurve, 0f, 100f);
        Utilities.ValidateCurve_Times(criticalChanceCurve, 0f, 100f);
        Utilities.ValidateCurve_Times(criticalMultiplierCurve, 0f, 100f);
    }
}
