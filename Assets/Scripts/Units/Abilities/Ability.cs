using UnityEngine;
using System.Collections;

public abstract class Ability : MonoBehaviour, IIdentifier {

    [SerializeField]
    string m_AbilityName = "Ability";

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
    float maxCharge = 0f;

    [Tooltip("Amount the ability charges over time")]
    [SerializeField]
    float chargeDeltaTime = 0f;

    [Tooltip("Amount the ability charges per unit of damage dealt")]
    [SerializeField]
    double chargeDeltaDamage = 0;

    [Tooltip("Amount the ability charges per kill")]
    [SerializeField]
    float chargeDeltaKill = 0f;

    protected float currentCharge = 0f;
    float chargeMultiplier = 1f;
    protected bool isAbilityActive = false;

    public delegate void AbilityChange(float _percent);
    public event AbilityChange OnAbilityChanged;



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
        return IsAbilityActive || (!IsAbilityActive && CurrentCharge - ActivationCost > 0);
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


    #region Accessors

    public InputType InputType
    {
        get { return activateInputType; }
    }

    public string Name
    {
        get { return m_AbilityName; }
        set { m_AbilityName = value; }
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
            currentCharge = Mathf.Clamp(value,0f,MaxCharge);

           if(OnAbilityChanged != null)
            {
                OnAbilityChanged(GetChargePercentage());
            }
        }
    }
    public float MaxCharge
    {
        get { return maxCharge; }
        protected set
        {
            maxCharge = value;

            if(maxCharge < 0)
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

            if(activationCost > MaxCharge)
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
    public override string ToString()
    {
        return m_AbilityName;
    }
}
