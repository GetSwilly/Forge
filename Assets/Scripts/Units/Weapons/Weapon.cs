using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Weapon : HandheldItem {

    protected static readonly float FIRE_RATE_MINIMUM = 0.01f;
    protected static readonly int NUMBER_RAYSWEEP_INCREMENTS = 50;

    [Space(15)]
    [Header("Targetting")]
    [Space(5)]

    [Tooltip("Layers to target")]
    [SerializeField]
    protected LayerMask targetMask;

    [Tooltip("Environment Layers")]
    [SerializeField]
    protected LayerMask environmentMask;

    [Tooltip("Layers to ignore")]
    [SerializeField]
    protected LayerMask friendlyMask;


    [Tooltip("Max angle Weapon can target")]
    [SerializeField]
    [Range(0f, 90f)]
    protected float maxSlope = 10f;


  
    [Space(15)]
    [Header("Stats")]
    [Space(5)]
    
    [Tooltip("Base Attack Power")]
    [SerializeField]
    int attackPower = 5;

    [Tooltip("Base Attack Range")]
    [SerializeField]
    float attackRange = 40f;

    [Tooltip("PRIMARY Attack Rate")]
    [SerializeField]
    float attackRatePrimary = 1;

    [Tooltip("SECONDARY Attack Rate")]
    [SerializeField]
    float attackRateSecondary = 1;
    


    [Tooltip("Base Critical Hit Chance")]
    [SerializeField]
    [Range(0f, 1f)]
    float baseCriticalHitChance = 0f;

    [Tooltip("Base Critical Hit Multiplier")]
    [SerializeField]
    [Range(0.25f, 4f)]
    float baseCriticalHitMultiplier = 1f;


    [Tooltip("Base Volume")]
    [SerializeField]
    [Range(0f, 1f)]
    float baseVolume = 1f;

    [Tooltip("Volume delta per level")]
    [SerializeField]
    [Range(0f, .25f)]
    float volumeBonusPerLevel = 0f;



    protected float attackTimerPrimary = 0;
    protected float attackTimerSecondary = 0;

    //  public delegate void WeaponEvent();
    //public event WeaponEvent OnAttackPrimary;
    //public event WeaponEvent OnAttackSecondary;
    //public event WeaponEvent OnReload;

    [Tooltip("Possible sounds for Critical Hit")]
    [SerializeField]
    protected List<WeightedObjectOfSoundClip> criticalAttackSounds = new List<WeightedObjectOfSoundClip>();




    public override void Awake()
    {
        base.Awake();

		attackTimerPrimary = 0f;
        attackTimerSecondary = 0f;
	}

   

	
	public virtual void Update()
    {
        if (attackTimerPrimary > 0)
        {
            attackTimerPrimary -= Time.deltaTime * (GetStatValue(StatType.Speed) + 1);
        }

        if (attackTimerSecondary > 0)
        {
            attackTimerSecondary -= Time.deltaTime * (GetStatValue(StatType.Speed) + 1);
        }
    }
	


    public override bool CanActivatePrimary()
    {
        return attackTimerPrimary <= 0;
	}
    public override bool CanActivateSecondary()
    {
        return attackTimerSecondary <= 0;
    }

    public virtual bool IsCritical()
    {
        return UnityEngine.Random.value <= CriticalChance;
    }
    

	/*
	public bool Use(UnitController _unit){
		_unit.PickupWeapon(this);
		return true;
	}
	
	public bool Give(UnitController _unit){
		return false;
	}*/


    public void SetVolume(int _level)
    {
        //float vol = baseVolume + (volumeBonusPerLevel * (_level-1));
        //vol = Mathf.Clamp(vol, baseVolume, 1f);

        //myAudio.volume = vol;
    }


    #region Accessors

    public LayerMask FriendlyMask
    {
        get { return friendlyMask; }
        set { friendlyMask = value; }
    }



    public virtual int AttackPower
    {
		get
        {
            int val = (int)((GetStatValue(StatType.Damage) + 1) * attackPower);
            return val;
        }
	}
	public virtual float AttackRange
    {
		get { return attackRange; }
	}
	public virtual float AttackRatePrimary
    {
		get
        {
            float rate = attackRatePrimary * (GetStatValue(StatType.Speed) + 1);

            return Mathf.Max(rate, FIRE_RATE_MINIMUM);
        }
	}
    public virtual float AttackRateSecondary
    {
        get
        {
            float rate = attackRateSecondary * (GetStatValue(StatType.Speed) + 1);

            return Mathf.Max(rate, FIRE_RATE_MINIMUM);
        }
    }



    public virtual float CriticalChance
    {
		get { return baseCriticalHitChance + GetStatValue(StatType.Luck); }
	}
	public virtual float CriticalMultiplier
    {
		get { return baseCriticalHitMultiplier * (GetStatValue(StatType.CriticalDamage) + 1) ; }
	}
    #endregion
}
