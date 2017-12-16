using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Identifier))]
public abstract class Ability : MonoBehaviour
{

    [SerializeField]
    protected bool showDebug = false;
    

    public enum ChargeType { Time, Damage, Kill };
    public ChargeType abilityCharger;


    [Tooltip("Methods of activating the ability")]
    [SerializeField]
    [EnumFlags]
    protected InputType activateInputType = InputType.Hold;



    [Space(5)]
    [Header("Ability Charge")]
    [Space(5)]

    [Tooltip("Charge cost for activating the ability")]
    [SerializeField]
    float activationCost = 0f;

    [Tooltip("Maximum possible charge for the ability")]
    [SerializeField]
    float maxCharge = 100f;

    [Tooltip("Amount the ability charges over time")]
    [SerializeField]
    float chargeDeltaTime = 0f;

    [Tooltip("Amount the ability charges per unit of damage dealt")]
    [SerializeField]
    double chargeDeltaDamage = 0;

    [Tooltip("Amount the ability charges per kill")]
    [SerializeField]
    float chargeDeltaKill = 0f;

    /*
    *******************************************************************************************************************************
    */
    [Header("Effects")]

    [SerializeField]
    protected SoundClip m_Sound;

    protected float currentCharge = 100f;
    float chargeMultiplier = 1f;
    protected bool isAbilityActive = false;
    
    public event Delegates.NamedValueChangeEvent OnAbilityChanged;

    protected Transform m_Transform;
    protected Identifier m_Identifier;
    AudioSource m_Audio;

    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Audio = GetComponent<AudioSource>();
        m_Identifier = GetComponent<Identifier>();
    }
    protected virtual void OnEnable()
    {
        CurrentCharge = CurrentCharge;
    }

    protected virtual void Update()
    {
        ChargeArithmetic(chargeDeltaTime * Time.deltaTime);
    }

    public abstract void Initialize(Transform _transform);
    public abstract void Terminate();

    public virtual void ActivateAbility()
    {
        ChargeArithmetic(-ActivationCost);
    }
    public abstract void DeactivateAbility();

    public virtual bool CanUseAbility()
    {
        return IsAbilityActive || (!IsAbilityActive && CurrentCharge - ActivationCost >= 0);
    }



    public void ChargeArithmetic(float _delta)
    {
        if (_delta > 0)
        {
            _delta *= ChargeMultiplier;
        }

        CurrentCharge += _delta;

    }



    public virtual void DamageAchieved(float damageDealt)
    {
        ChargeArithmetic((float)(chargeDeltaDamage * damageDealt));
    }
    public virtual void KillAchieved()
    {
        ChargeArithmetic(chargeDeltaKill);
    }

    public bool HasCharge()
    {
        return Mathf.FloorToInt(CurrentCharge) > 0;
    }

    public virtual void SetCharge(float amount)
    {
        currentCharge = amount;
        ChargeArithmetic(0f);
    }

    public float GetChargePercentage()
    {
        if (maxCharge.Equals(0f))
        {
            return 0f;
        }

        return Mathf.Clamp01(currentCharge / maxCharge);
    }

    protected void PlaySound(SoundClip sound)
    {
        if (sound.UseRemnant)
        {
            GameObject remnantObject = ObjectPoolerManager.Instance.AudioRemnantPooler.GetPooledObject();
            AudioRemnant remnantScript = remnantObject.GetComponent<AudioRemnant>();

            if (remnantScript != null)
            {
                remnantObject.transform.position = m_Transform.position;
                remnantObject.SetActive(true);

                remnantScript.PlaySound(sound);
            }
        }
        else
        {
            m_Audio.volume = sound.Volume;
            m_Audio.pitch = sound.Pitch;

            if (sound.IsLooping)
            {
                m_Audio.loop = true;
                m_Audio.clip = sound.Sound;
                m_Audio.Play();
            }
            else
            {
                m_Audio.loop = false;
                m_Audio.PlayOneShot(sound.Sound);
            }
        }
    }

    #region Accessors

    public string Name
    {
        get { if (m_Identifier == null)
            {
                Debug.Log("NULL IDENTIFIER");
                Debug.Log(this.gameObject.name);
            }
            return m_Identifier.Name; }
    }

    public InputType InputType
    {
        get { return activateInputType; }
    }
    
    public bool IsAbilityActive
    {
        get { return isAbilityActive; }
        protected set { isAbilityActive = value; }
    }

    public float CurrentCharge
    {
        get { return currentCharge; }
        private set
        {
            currentCharge = Mathf.Clamp(value, 0f, MaxCharge);

            if (OnAbilityChanged != null)
            {
                OnAbilityChanged(m_Identifier.Name, GetChargePercentage());
            }
        }
    }
    public float MaxCharge
    {
        get { return maxCharge; }
        protected set
        {
            maxCharge = value;

            if (maxCharge < 0)
            {
                maxCharge = 0;
            }
        }
    }

    public float ActivationCost
    {
        get { return activationCost; }
        set
        {
            activationCost = value;

            if (activationCost < 0)
            {
                activationCost = 0;
            }

            if (activationCost > MaxCharge)
            {
                activationCost = MaxCharge;
            }
        }
    }
    public float ChargeMultiplier
    {
        get { return chargeMultiplier; }
        set { chargeMultiplier = value; }
    }

    #endregion


    protected virtual void OnValidate()
    {
        MaxCharge = MaxCharge;
        ActivationCost = ActivationCost;
    }
}
