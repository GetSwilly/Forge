using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AudioSource))]
public class Health : MonoBehaviour
{

	static readonly float LOW_HEALTH_PERCENT = 0.2f;
    //static readonly float INVINCIBILITY_TIME = 0.2f;
    
    float currentHealth;

    [Tooltip("Maximum possible health")]
    [SerializeField]
    int baseMaxHealth; 
	int additionalHealth = 0;
    
    [Tooltip("Minimum amount of damage necessary to cause injury greater than 1")]
    [SerializeField]
    int damageResistance;

    [Tooltip("Amount of time, upon taking damage, when unit is invincible ")]
    [SerializeField]
    float invincibilityTime = 0.2f;

	bool isAlive;
	float lastHealthChange;
	Transform lastAttacker;
	Vector3 lastAttackDirection;

    [SerializeField]
	bool isInvincible = false;
    
    [Tooltip("Should the unit receive Maximum Health on activation?")]
    [SerializeField]
    bool maxHealthOnActive = true;

    [Tooltip("Should the unit deactive?")]
    [SerializeField]
    bool deactivateOnDeath = true;

	public delegate void AlertHealthChange(Health healthScript);
	public AlertHealthChange OnHealthChange;
	public AlertHealthChange OnDamaged;
	public AlertHealthChange OnKilled;

    [SerializeField]
    AudioClip damagedSound;

    [SerializeField]
    AudioClip deathSound;


    [SerializeField]
    DisplayEffect healthGainedEffect;

    [SerializeField]
    DisplayEffect healthLostEffect;

    [SerializeField]
    DisplayEffect deathEffect;


    [SerializeField]
    bool showDebug = false;

	AudioSource m_Audio;
   

	void Awake()
    {
		m_Audio = GetComponent<AudioSource>();

		Initialize();
	}

	void OnEnable()
    {
		if(maxHealthOnActive)
			Initialize();
	}







	void Initialize()
    {
		StopAllCoroutines();
		//StopEffects();
		currentHealth = MaxHealth;
		isAlive = true;
		isInvincible = false;
	}


	//void StartEffects(AudioClip soundClip)
 //   {
	//	if(soundClip != null)
 //       {
	//		StopEffects();
	//		myAudio.PlayOneShot(soundClip);
	//	}
	//}
	//void StopEffects()
 //   {
	//	myAudio.Stop();
	//}




	//Add amount to currentHealth and sign the attack with a transform and (optional) direction
	public void HealthArithmetic (float dmgAmount, bool isCritical, Transform attackerTransform)
    {
		HealthArithmetic(dmgAmount, isCritical, attackerTransform, Vector3.zero);
	}
	public void HealthArithmetic (float dmgAmount, bool isCritical, Transform attackerTransform, Vector3 attackDirection)
    {
		if(!isAlive || isInvincible)
			return;

		if(dmgAmount < 0)
        {
			//Sign the attack
			lastAttacker = attackerTransform;
			lastAttackDirection = attackDirection;
		}

        if (ObjectPoolerManager.Instance != null)
        {
            GameObject _info = ObjectPoolerManager.Instance.DynamicInfoPooler.GetPooledObject();
            _info.transform.position = transform.position;

            DynamicInfoScript _infoScript = _info.GetComponent<DynamicInfoScript>();
            //_infoScript.infoText.text = Mathf.Abs(dmgAmount).ToString();
            Color infoColor = isCritical ? Color.yellow : Color.white;

            _info.SetActive(true);

            _infoScript.Initialize(Mathf.Abs(dmgAmount).ToString(), infoColor);

        }



		//Check if can resist damage
		if(dmgAmount < 0 && Mathf.Abs(dmgAmount) <= damageResistance)
			dmgAmount = 1;

        if (showDebug)
        {
            Debug.Log(string.Format("{0} -- Health Arithmetic. Current Health: {1}. Delta: {2}. Last Attacker: {3}.", this.name, CurHealth, dmgAmount, LastAttacker));
        }

		//Add to currentHealth and contain it
		CurHealth += dmgAmount;
		lastHealthChange = dmgAmount;
	
		if(CurHealth <= 0)
        {
			Death();
		}
        else
        {
			if (dmgAmount < 0)
            {
				if(OnDamaged != null)
					OnDamaged(this);

                //StartEffects(damagedSound);
                if (healthLostEffect != null)
                {
                    healthLostEffect.Play();
                }
			}
            else
            {
                if (healthGainedEffect != null)
                {
                    healthGainedEffect.Play();
                }
            }
			
			if(OnHealthChange != null)
				OnHealthChange(this);

			StopCoroutine(TemporaryInvincibility());
			StartCoroutine(TemporaryInvincibility());
		}
	}
	
	public void ReviveMax()
    {
		Revive(MaxHealth);
	}
	public void Revive(float newHealth)
    {
		isAlive = true;
		currentHealth = newHealth;
		lastAttacker = null;
		lastAttackDirection = Vector3.zero;
        lastHealthChange = 0;

		if(OnHealthChange != null)
			OnHealthChange(this);

		if(gameObject.activeSelf)
        {
			StopCoroutine(TemporaryInvincibility());
			StartCoroutine(TemporaryInvincibility());
		}
	}

	public void Death ()
    {
		// Set the death flag so this function won't be called again.
		isAlive = false;
		StopAllCoroutines();

		GameObject go = ObjectPoolerManager.Instance.DeathAnimationPooler.GetPooledObject();
		go.transform.position = this.transform.position;

		go.GetComponent<DeathParticles>().deathSound = deathSound;

		//go.GetComponent<ParticleSystem>().startColor = GetComponent<SpriteRenderer>().color;

		go.SetActive(true);


       if(deathEffect != null)
        {
            deathEffect.Play();
        }
	

		if(OnDamaged != null)
			OnDamaged(this);

		if(OnHealthChange != null)
			OnHealthChange(this);

		if (OnKilled != null)
			OnKilled(this);
        


        if(deactivateOnDeath)
            gameObject.SetActive(false);

	}

	IEnumerator TemporaryInvincibility()
    {
		isInvincible = true;

		yield return new WaitForSeconds(InvincibilityTime);
        
		isInvincible = false;
	}

    public bool CanBeDamaged()
    {
        return CanBeDamaged(0);
    }
    public bool CanBeDamaged(float dmg)
    {
        return (isAlive && !isInvincible && Mathf.Abs(dmg) > damageResistance);
    }


    #region Accessors

    public float CurHealth
    {
		get { return currentHealth; }
		private set
        {
            currentHealth = Mathf.Clamp(value, 0, MaxHealth);

        }
	}
	public int MaxHealth
    {
		get { return baseMaxHealth + AdditionalHealth; }
		set { baseMaxHealth = Mathf.Clamp(value, 0, value); ; }
	}
	public int AdditionalHealth
    {
		get { return additionalHealth; }
		set { additionalHealth = value; }
	}
	public int DamageResistance
    {
		get { return damageResistance; }
		set { damageResistance = Mathf.Clamp(value, 0, value); ; }
	}
    public float InvincibilityTime
    {
        get { return invincibilityTime; }
        set { invincibilityTime = Mathf.Clamp(value, 0, value); }
    }
	public Transform LastAttacker
    {
		get { return lastAttacker; }
	}
	public Vector3 LastAttackDirection
    {
		get { return lastAttackDirection.normalized; }
	}
	public float LastHealthChange
    {
		get { return lastHealthChange; }
	}


	
	public bool IsAlive
    {
		get { return isAlive; }
	}
	public bool IsLowHealth
    {
		get { return HealthPercentage <= LOW_HEALTH_PERCENT; }
	}
	public bool NeedsHealth
    {
		get { return CurHealth < MaxHealth; }
	}
	
	public float HealthPercentage
    {
		get { return CurHealth / (float)MaxHealth; }
	}
	#endregion

    void OnValidate()
    {
        MaxHealth = MaxHealth;
        InvincibilityTime = InvincibilityTime;
        DamageResistance = DamageResistance;
    }
}
