using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using mattatz.VoxelSystem;

[RequireComponent(typeof(AudioSource))]
public class Health : MonoBehaviour, IMemorable
{
    static readonly Color _HealthLostColor = Color.white;
    static readonly Color _HealthGainedColor = Color.red;
    static readonly Color _CriticalHitColor = Color.cyan;
    static readonly float _LowHealthPercentage = 0.2f;
    static readonly float _FlashDamageTime = .2f;
    //static readonly float INVINCIBILITY_TIME = 0.2f;

    [SerializeField]
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

    bool isAlive = true;
    bool wasLastAttackCritical;
    float lastHealthChange;
    Transform lastAttacker;
    Vector3 lastAttackDirection;

    [SerializeField]
    bool isInvincible = false;

    [Tooltip("Should the unit receive Maximum Health on activation?")]
    [SerializeField]
    bool maxHealthOnActive = true;

    [SerializeField]
    bool shouldVoxelizeOnDeath = true;

    [SerializeField]
    bool showDebug = false;


    public delegate void AlertHealthChange(Health healthScript);
    public AlertHealthChange OnHealthChange;
    public AlertHealthChange OnDamaged;
    public AlertHealthChange OnKilled;

    Voxelizer m_Voxelizer;


    void Awake()
    {
        m_Voxelizer = GetComponent<Voxelizer>();
    }
    void OnEnable()
    {
        if (maxHealthOnActive)
            Initialize();
    }


    void Initialize()
    {
        StopAllCoroutines();

        CurrentHealth = MaxHealth;
        IsAlive = true;
        IsInvincible = false;
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
    public void HealthArithmetic(float healthDelta, bool isCritical, Transform attackerTransform)
    {
        HealthArithmetic(healthDelta, isCritical, attackerTransform, Vector3.zero);
    }
    public void HealthArithmetic(float healthDelta, bool isCritical, Transform attackerTransform, Vector3 attackDirection)
    {
        if (!IsAlive || IsInvincible)
        {
            return;
        }

        if (healthDelta < 0)
        {
            //Sign the attack
            lastAttacker = attackerTransform;
            lastAttackDirection = attackDirection;
        }

        if (healthDelta > 0 && CurrentHealth >= MaxHealth)
        {
            return;
        }

        int roundedValue = Mathf.RoundToInt(healthDelta);

        if (!roundedValue.Equals(0))
        {
            Color infoColor = roundedValue > 0f ? _HealthGainedColor : (isCritical ? _CriticalHitColor : _HealthLostColor);
            UIManager.Instance.CreateDynamicInfoScript(transform.position, roundedValue, infoColor);
        }



        //Check if can resist damage
        if (healthDelta < 0 && Mathf.Abs(healthDelta) <= DamageResistance)
            healthDelta = 1;

        if (showDebug)
        {
            Debug.Log(string.Format("{0} -- Health Arithmetic. Current Health: {1}. Delta: {2}. Last Attacker: {3}.", this.name, CurrentHealth, healthDelta, LastAttacker));
        }

        //Add to currentHealth and contain it
        CurrentHealth += healthDelta;
        LastHealthChange = healthDelta;
        WasLastAttackCritical = isCritical;

        if (CurrentHealth <= 0)
        {
            Death();
        }
        else
        {
            IsAlive = true;

            if (healthDelta < 0)
            {
                if (OnDamaged != null)
                    OnDamaged(this);
            }

            if (OnHealthChange != null)
                OnHealthChange(this);


            if (healthDelta < 0)
            {
                StopCoroutine(FlashDamageColor());
                StartCoroutine(FlashDamageColor());

                StopCoroutine(TemporaryInvincibility());
                StartCoroutine(TemporaryInvincibility());
            }
        }
    }

    public void ReviveMax()
    {
        Revive(MaxHealth);
    }
    public void Revive(float newHealth)
    {
        IsAlive = true;
        CurrentHealth = newHealth;

        lastAttacker = null;
        lastAttackDirection = Vector3.zero;
        lastHealthChange = 0;

        if (OnHealthChange != null)
            OnHealthChange(this);

        if (gameObject.activeSelf)
        {
            StopCoroutine(TemporaryInvincibility());
            StartCoroutine(TemporaryInvincibility());
        }
    }

    public void Death()
    {
        // Set the death flag so this function won't be called again.
        IsAlive = false;
        StopAllCoroutines();


        if (OnDamaged != null)
            OnDamaged(this);

        if (OnHealthChange != null)
            OnHealthChange(this);

        if (OnKilled != null)
            OnKilled(this);


        if (shouldVoxelizeOnDeath && m_Voxelizer != null)
        {
            m_Voxelizer.Activate();
        }

        gameObject.SetActive(false);
    }

    IEnumerator FlashDamageColor()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[] colors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            colors[i] = renderers[i].material.HasProperty("_Color") ? renderers[i].material.color : Color.white;

            renderers[i].material.color = _HealthLostColor;
        }

        yield return new WaitForSeconds(_FlashDamageTime);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = colors[i];
        }
    }
    IEnumerator TemporaryInvincibility()
    {
        IsInvincible = true;

        yield return new WaitForSeconds(InvincibilityTime);

        IsInvincible = false;
    }

    public bool CanBeDamaged()
    {
        return CanBeDamaged(0);
    }
    public bool CanBeDamaged(float dmg)
    {
        return (IsAlive && !IsInvincible && Mathf.Abs(dmg) > DamageResistance);
    }


    #region Accessors

    public GameObject GameObject
    {
        get { return this.gameObject; }
    }
    public Transform Transform
    {
        get { return this.transform; }
    }
    public float CurrentHealth
    {
        get { return currentHealth; }
        private set { currentHealth = Mathf.Clamp(value, 0, MaxHealth); }
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
    public bool WasLastAttackCritical
    {
        get { return wasLastAttackCritical; }
        private set { wasLastAttackCritical = value; }
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
        private set { lastHealthChange = value; }
    }


    public bool IsInvincible
    {
        get { return isInvincible; }
        private set { isInvincible = value; }
    }
    public bool IsAlive
    {
        get { return isAlive; }
        private set { isAlive = value; }
    }
    public bool IsLowHealth
    {
        get { return HealthPercentage <= _LowHealthPercentage; }
    }
    public bool NeedsHealth
    {
        get { return CurrentHealth < MaxHealth; }
    }

    public float HealthPercentage
    {
        get { return CurrentHealth / (float)MaxHealth; }
    }

    #endregion

    void OnValidate()
    {
        CurrentHealth = CurrentHealth;
        MaxHealth = MaxHealth;
        InvincibilityTime = InvincibilityTime;
        DamageResistance = DamageResistance;
    }
}
