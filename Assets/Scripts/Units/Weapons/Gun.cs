using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AmmoSystem))]
public class Gun : Weapon
{

    static readonly float MINIMUM_FIRE_RATE = 0.1f;
    static readonly float MUZZLE_FLASH_TIME = 0.1f;


    [Header("Shots")]

    [SerializeField]
    ShotContainer m_Shot;

    ObjectPooler primaryPool;
    GameObject primaryProjector;
    IProjectile primaryProjectorScript;

    /*
    *******************************************************************************************************************************
    */
    [Header("Misc.")]

    [SerializeField]
    Transform gunBarrelTransform;


    [SerializeField]
    float shotSpeed = 25f;
    [SerializeField]
    float reloadTime = 1f;

    //[SerializeField]
    //[Range(0f, 100f)]
    //float reloadCriticalSize = 10f;

    bool isFiring = false;
    bool isReloading = false;
    float curReloadTime = 0f;

    [SerializeField]
    GameObject muzzleFlash;


    List<GameObject> bulletParents = new List<GameObject>();

    /*
    *******************************************************************************************************************************
    */
    [Header("Attack Charge")]

    [SerializeField]
    [Range(0f, 100f)]
    float requiredCharge = 0f;

    [SerializeField]
    float primaryChargeRate;

    [SerializeField]
    float passiveChargeRate = 0f;

    float currentCharge = 0f;


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



    void InitializeObjectPools()
    {
        GameObject p;
        GameObject poolsObj = GameObject.Find("Pools");


        if (primaryPool == null)
            primaryPool = gameObject.AddComponent<ObjectPooler>();


        p = new GameObject(m_Transform.parent.name + " Projectile Pooler -- Primary");


        if (poolsObj != null)
        {
            p.transform.parent = poolsObj.transform;
        }

        primaryPool.Parent = p.transform;
        primaryPool.PooledObject = m_Shot.ProjectileObject;
        primaryPool.Initialize();
        bulletParents.Add(p);

        if (m_Shot.Ammo != null)
            m_Shot.Ammo.OnAmmoChanged += UpdateAmmo;
    }

    public override void Update()
    {
        base.Update();


        //if (!isFiring)
        // {
        CurrentCharge += PassiveChargeRate * Time.deltaTime * (GetStatValue(StatType.Speed) + 1);
        // }
    }



    public override void Initialize(Transform owner, Team team)
    {
        base.Initialize(owner, team);

        DestroyBulletObjects();
        isReloading = false;

        InitializeObjectPools();

        CurrentCharge = 0f;

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
        }
    }
    public override void Terminate()
    {
        base.Terminate();


        DestroyBulletObjects();

        m_Shot.Ammo.OnAmmoChanged -= UpdateAmmo;
        //secondaryAmmo.OnAmmoChanged -= UpdateAmmo;

        StopAllCoroutines();

        if(muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
        }
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
            for (int k = 0; k < hits.Length; k++)
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

        if (m_Movement != null)
        {
            m_Movement.AddSpeedMultiplier(this, isPrimary ? primarySpeedSpeedup : secondarySpeedSpeedup);
            m_Movement.AddRotationMultiplier(this, isPrimary ? primaryRotationSpeedup : secondaryRotationSpeedup);
        }


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

                bScript.Initialize(m_Owner, m_Team, fireDir, (int)-_power, isCrit, _speed, _range);
                bScript.SubscribeToOnImpact(AlertHealthChangeCaused);

                EnableEffects();


                PlaySound(_container.Sound);


                if (isCrit)
                {
                    //	PlaySound(criticalShotSound);
                }

                _container.Ammo.UseAmmo(_shots[i].AmmoCost);

            }

        }

        if (m_Movement != null)
        {
            m_Movement.RemoveSpeedMultiplier(this);
            m_Movement.RemoveRotationMultiplier(this);
        }


        isFiring = false;



        if (isPrimary)
        {
            attackTimerPrimary = AttackRatePrimary;
        }
        else
        {
            attackTimerSecondary = AttackRateSecondary;
        }

        
        CurrentCharge += _container.ChargeEffect;
    }



    public override void ActivatePrimary()
    {
        CurrentCharge += primaryChargeRate * Time.deltaTime;
        
        if (m_Shot.CanShoot() && CurrentCharge >= RequiredCharge)
        {
            DisableEffects();

            if (isReloading)
            {
                StopAllCoroutines();
                isReloading = false;
                AlertHandheldUpdate(GetPercentage());
            }

            //myAudio.pitch = pitchCurve.Evaluate(currentDeterioration);
            StartCoroutine(FireProjectile(m_Shot, primaryPool, true));
        
            AlertPrimaryActivation();

        }
        else
        {
            Reload();
        }
    }
    public override bool CanActivatePrimary()
    {
        return base.CanActivatePrimary() && !isFiring;
    }


    public override void ActivateSecondary() { }
    public override bool CanActivateSecondary()
    {
        return base.CanActivateSecondary() && !isFiring;
    }


    public override void ActivateTertiary()
    {
        Reload();
    }

    void Reload()
    {
        AmmoSystem _ammo = m_Shot.Ammo;

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

        AlertHandheldUpdate(0f);

        while (curReloadTime < reloadTime)
        {
            curReloadTime += Time.deltaTime;


            AlertHandheldUpdate(GetPercentage());

            yield return null;
        }

        curReloadTime = reloadTime;

        if (a.CanReload())
        {
            a.Reload();
        }

        isReloading = false;
    }



   protected void EnableEffects()
    {
        bool isCriticalHit = false;

        SoundClip _sound;//isCriticalHit ? criticalShotSound : normalShotSound;   //Utilities.WeightedSelection( shotSounds.ToArray(), shotSoundProbabilities.ToArray());

        if (isCriticalHit)
        {
            _sound = Utilities.WeightedSelection(criticalAttackSounds.ToArray(), 0f);
        }
        else
        {
            _sound = Utilities.WeightedSelection(primaryActionSounds.ToArray(), 0f);
        }

        PlaySound(_sound);

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
            muzzleFlash.SetActive(true);
        }

        StartCoroutine(DelayDisableEffects());
    }
    protected void DisableEffects()
    {
        //myAudio.Stop();

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(false);
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


        return m_Shot.Ammo == null ? 0f : m_Shot.Ammo.GetAmmoPercentage();
    }

    /*
	void OnDestroy(){
		Destroy(bulletPooler.Parent.gameObject);
		Destroy(this.gameObject);
	}*/


    void UpdateAmmo(float _percent)
    {
        AlertHandheldUpdate(_percent);
    }

    public override void DeactivatePrimary() { }
    public override void DeactivateSecondary() { }
    public override void DeactivateTertiary() { }

    public override bool CanActivateTertiary()
    {
        return m_Shot.Ammo != null && m_Shot.Ammo.GetAmmoPercentage() < 100f;
    }



    #region Accessors


    public override int AttackPower
    {
        get { return (int)(powerCurve.Evaluate(CurrentCharge)); }
    }
    public override float AttackRange
    {
        get { return rangeCurve.Evaluate(CurrentCharge); }
    }
    public override float AttackRatePrimary
    {
        get
        {
            float val = rateCurve.Evaluate(CurrentCharge);
            return val > MINIMUM_FIRE_RATE ? val : MINIMUM_FIRE_RATE;
        }
    }
    public override float AttackRateSecondary
    {
        get
        {
            float val = rateCurve.Evaluate(CurrentCharge);
            return val > MINIMUM_FIRE_RATE ? val : MINIMUM_FIRE_RATE;
        }
    }

    public override float CriticalChance
    {
        get { return criticalChanceCurve.Evaluate(CurrentCharge); }
    }
    public override float CriticalMultiplier
    {
        get { return criticalMultiplierCurve.Evaluate(CurrentCharge); }
    }


    public float Accuracy
    {
        get { return accuracyCurve.Evaluate(CurrentCharge); }
    }
    public float ShotSpeed
    {
        get { return shotSpeedCurve.Evaluate(CurrentCharge); }
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
    protected float RequiredCharge
    {
        get { return requiredCharge; }
        private set { requiredCharge = Mathf.Clamp(value, 0f, 100f); }
    }
    protected float PassiveChargeRate
    {
        get { return passiveChargeRate; }
        set { passiveChargeRate = value; }
    }
    protected float CurrentCharge
    {
        get { return currentCharge; }
        set
        {
            currentCharge = Mathf.Clamp(value, 0f, 100f);
        }
    }

    public float ReloadTime
    {
        get { return reloadTime; }
        set
        {
            reloadTime = value;

            if (reloadTime <= 0f)
            {
                reloadTime = 0.001f;
            }
        }
    }
    public bool IsReloading
    {
        get { return isReloading; }
    }


    protected Vector3 GunBarrelPosition
    {
        get { return gunBarrelTransform == null ? m_Transform.position : gunBarrelTransform.position; }
    }

    #endregion


    protected override void OnValidate()
    {
        base.OnValidate();

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
