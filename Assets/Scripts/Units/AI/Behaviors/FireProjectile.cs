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
    
    [SerializeField]
    [Range(0f,1f)]
    float baseCriticalHitChance;

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

                bool isCrit = UnityEngine.Random.value <= CriticalHitChance;

                float _power = AttackPower;
                _power *= isCrit ? CriticalHitMultiplier : 1f;
                _power *= shots[i].PowerModifier;

                float _speed = shotSpeed * shots[i].SpeedModifier;

                float _range = AttackRange * shots[i].RangeModifier;

                pScript.Initialize(m_Transform.parent, m_Team, fireDir, -(int)_power, isCrit, _speed, _range);

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
        //Null Target?
        if (m_Actor.TargetObject == null)
        {
            if (ShowDebug)
            {
                Debug.Log("FireProjectile --- Null Target");
            }

            return 0f;
        }
        
        //Target in range?
        if (Vector3.Distance(m_Transform.position, m_Actor.TargetObject.LastKnownBasePosition) > AttackRange) //|| !m_Actor.CanSee(m_Actor.TargetObject.LastKnownPosition))
        {
            if (ShowDebug)
            {
                Debug.Log("FireProjectile --- Target not within range");
            }

            return 0f;
        }

        //Find best sub-target score
        float maxScore = 0f;
        m_Actor.TargetObject.LastKnownPositions.ForEach(t =>
        {
            float score = GetTargetScore(t);
            maxScore = Mathf.Max(score, maxScore);
        });
        

        if (ShowDebug)
        {
            Debug.Log("FireProjectile -- Score: " + maxScore);
        }

        return maxScore;
    }

    /// <summary>
    /// Calculate the score for aiming at a position;
    /// </summary>
    float GetTargetScore(Vector3 position)
    {
        Vector3 exactDirection = position - m_Transform.TransformPoint(gunBarrelOffset);
        exactDirection.y = 0f;


        float averageAnglePercentage = 0f;
        for (int i = 0; i < shots.Count; i++)
        {
            averageAnglePercentage += Vector3.Angle(m_Transform.TransformDirection(shots[i].LocalDirection), exactDirection) / maxAttackAngle;
        }
        averageAnglePercentage /= shots.Count;

        float percentage = Mathf.Clamp01(averageAnglePercentage);  //Mathf.Clamp01(angle/maxAttackAngle);


        float score = utilityCurve.Evaluate(percentage);

        if (ShowDebug)
        {
            Debug.Log("FireProjectile -- Score: " + score);
        }

        return score;
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
        get { return baseAttackPower; }
        set { baseAttackPower = Mathf.Clamp(value, 0f, value); }
    }
    public float CriticalHitChance
    {
        get { return baseCriticalHitChance; }
        set { baseCriticalHitChance = Mathf.Clamp(value, 0f, value); }
    }
    public float CriticalHitMultiplier
    {
        get { return baseCriticalHitMultiplier; }
        set { baseCriticalHitMultiplier = Mathf.Clamp(value, 0f, value); }
    }
    public float AttackRange
    {
        get { return baseAttackRange; }
        set { baseAttackRange = Mathf.Clamp(value, 0f, value); }
    }


    protected override void OnValidate()
    {
        base.OnValidate();

        AttackPower = AttackPower;
        AttackRange = AttackRange;
        CriticalHitChance = CriticalHitChance;
        CriticalHitMultiplier = CriticalHitMultiplier;

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
