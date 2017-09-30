using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FireProjectile : BaseUtilityBehavior {

    [Tooltip("Relative maximum angle can fire at")]
    [SerializeField]
    [Range(0f, 180f)]
    float maxAttackAngle = 20f;

    [Tooltip("Base damage of attack")]
    [SerializeField]
    float baseAttackPower;

    [Tooltip("Base critical hit multiplier of the attack")]
    [SerializeField]
    float baseCriticalHitMultiplier;

    [Tooltip("Base range of the attack")]
   [SerializeField]
    float baseAttackRange;

    [Tooltip("Speed of fired projectiles")]
    [SerializeField]
    float shotSpeed;

    [Tooltip("Speed of fired projectiles")]
    [SerializeField]
    List<Shot> shots = new List<Shot>();

    [SerializeField]
    GameObject projectilePrefab;

    [SerializeField]
    Vector3 gunBarrelOffset = Vector3.zero;
    
    [SerializeField]
    AudioClip shotSound;


    
    ObjectPooler projectilePool;


    void Start()
    {
        GameObject genObj = GameObject.Find("Pools");
    

        GameObject projectileParent = new GameObject(m_Transform.name + " -- FireProjectile -- Projectile Pooler");
        projectilePool = projectileParent.AddComponent<ObjectPooler>();


        if (genObj != null)
            projectileParent.transform.parent = genObj.transform;

        projectilePool.Parent = projectileParent.transform;
        projectilePool.PooledObject = projectilePrefab;
        projectilePool.PoolLength = 5;
        projectilePool.Initialize();
    }


    IEnumerator Fire()
    {
        if (m_Actor.ShowDebug)
        {
            Debug.Log(Time.time + " #### " + m_Transform.name + " -- FireProjectile -- Firing " + shots.Count + " shots.");
        }


        for (int i = 0; i < shots.Count; i++)
        {
            yield return new WaitForSeconds(shots[i].Delay);


            Vector3 localDir = m_Transform.TransformDirection(shots[i].LocalDirection);


            // Vector3 shotError = new Vector3(-localDir.z, 0, localDir.x);

            Vector3 fireDir = localDir; // * currentAccuracy) + shotError;
            fireDir.Normalize();

          
            GameObject _projectile = projectilePool.GetPooledObject();
            _projectile.transform.position = m_Transform.TransformPoint(gunBarrelOffset);// +myTransform.TransformPoint(_shot[i].LocalOffset);
            _projectile.transform.rotation = m_Transform.rotation;

            IProjectile pScript = _projectile.GetComponent<IProjectile>();

            if (pScript != null)
            {
                _projectile.SetActive(true);

                bool isCrit = UnityEngine.Random.value <= m_Actor.CriticalHitChance;

                float _power = AttackPower;
                _power *= isCrit ? CriticalHitMultiplier : 1f;
                _power *= shots[i].PowerModifier;

                float _speed = shotSpeed * shots[i].SpeedModifier;

                float _range = AttackRange * shots[i].RangeModifier;

                pScript.Initialize(m_Transform.parent, m_Actor.GetTeam(), fireDir, -(int)_power, isCrit, _speed, _range);

                if (ShowDebug)
                {
                    Debug.DrawLine(m_Transform.position, m_Transform.position + (fireDir * _range), Color.red, 2f);
                }
            }

        }


        EndBehavior(true, true);
    }



    public override void StartBehavior()
    {
        IsActive = true;

        StartCoroutine(Fire());
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        StopAllCoroutines();

        base.EndBehavior(shouldNotifySuper, shouldNotifyActor);
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }


    public override float GetBehaviorScore()
    {
        if (m_Actor.TargetObject == null)
            return 0f;

        float totalAngle = 0f;
        for (int i = 0; i < shots.Count; i++)
        {
            totalAngle += Vector3.Angle(m_Transform.TransformDirection(shots[i].LocalDirection), m_Actor.TargetObject.LastKnownPosition - m_Transform.position) / maxAttackAngle;
        }
        totalAngle /= shots.Count;

        //float angle = Vector3.Angle(myTransform.forward, myActor.TargetTransform.position - myTransform.position);
        float percentage = Mathf.Clamp01(totalAngle);  //Mathf.Clamp01(angle/maxAttackAngle);


        return utilityCurve.Evaluate(percentage);
    }
    


    public override bool CanEndBehavior
    {
        get { return true; }
    }

    public override bool CanStartSubBehavior
    {
        get { return false; }
    }
    



    public float AttackPower
    {
        get { return baseAttackPower * (1f + m_Actor.BonusAttackPower); }
    }
    public float CriticalHitMultiplier
    {
        get { return baseCriticalHitMultiplier * (1f + m_Actor.BonusCriticalHitMultiplier); }
    }
    public float AttackRange
    {
        get { return baseAttackRange * (1f + m_Actor.BonusAttackRange); }
    }


    protected override void OnValidate()
    {
        base.OnValidate();

        baseAttackPower = Mathf.Clamp(baseAttackPower, 0f, baseAttackPower);
        baseCriticalHitMultiplier = Mathf.Clamp(baseCriticalHitMultiplier, 0f, baseCriticalHitMultiplier);
        baseAttackRange = Mathf.Clamp(baseAttackRange, 0f, baseAttackRange);

        Utilities.ValidateCurve_Times(utilityCurve, 0f, 1f);

        for(int i = 0; i < shots.Count; i++)
        {
            shots[i].Validate();
        }
    }



    public override string ToString()
    {
        return "FireProjectile";
    }
}
