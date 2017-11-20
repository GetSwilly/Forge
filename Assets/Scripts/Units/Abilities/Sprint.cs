using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(TrailRenderer))]
public class Sprint : Ability, IMovementAffector {

    static readonly float MINIMUM_ACTIVATION_DELAY = .1f;


    [Tooltip("Amount of charge to be used for each second the ability is active")]
    [SerializeField]
    float m_ChargeUseDelta = 0f;


    [Space(5)]
    [Header("Movement Multipliers")]
    [Space(5)]

    [Tooltip("Multiplier to be applied to Movement Speed while ability is being used")]
    [SerializeField]
    [Range(0.25f, 3f)]
    float speedMultiplier = 1f;

    [Tooltip("Multiplier to be applied to Rotation Speed while ability is being used")]
    [SerializeField]
    [Range(0f, 1f)]
    float rotationMultiplier = 1f;


    bool canSprint = true;
    
    MovementController unitMovement;
    TrailRenderer m_TrailRenderer;
     
    protected override void Awake()
    {
        base.Awake();

        m_TrailRenderer = GetComponent<TrailRenderer>();
        m_TrailRenderer.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        if (IsAbilityActive)
        {
            if (HasCharge())
            {
                ChargeArithmetic(Time.deltaTime * m_ChargeUseDelta);
            }
            else
            {
                DeactivateAbility();
            }
        }
    }

    public override void Initialize(Transform _transform)
    {
        unitMovement = _transform.GetComponent<MovementController>();

        if (unitMovement == null)
            this.enabled = false;

    }
    public override void Terminate()
    {
        DeactivateAbility();
        unitMovement = null;
    }


    public override void ActivateAbility()
    {
        if (IsAbilityActive)
            return;
        
        EnableMovementEffects();
        IsAbilityActive = true;
    }
    public override void DeactivateAbility()
    {
        if (!IsAbilityActive)
            return;


        DisableMovementEffects();
        IsAbilityActive = false;
        StartCoroutine(ActivationDelay());
    }
    public override bool CanUseAbility()
    {
        bool baseTest = IsAbilityActive || (!IsAbilityActive && CurrentCharge - (ActivationCost* Time.deltaTime) >= 0);
  
        return baseTest && CanSprint;
    }


    IEnumerator ActivationDelay()
    {
        CanSprint = false;

        yield return new WaitForSeconds(MINIMUM_ACTIVATION_DELAY);

        CanSprint = true;
    }


    public void EnableMovementEffects()
    {
        m_TrailRenderer.enabled = true;
        StartCoroutine(ResetTrail());

        unitMovement.AddSpeedMultiplier(this, speedMultiplier);
        unitMovement.AddRotationMultiplier(this, rotationMultiplier);
    }

    public void DisableMovementEffects()
    {
        m_TrailRenderer.enabled = false;
        unitMovement.RemoveSpeedMultiplier(this);
        unitMovement.RemoveRotationMultiplier(this);
    }
    IEnumerator ResetTrail()
    {

        float trailTime = m_TrailRenderer.time;
        m_TrailRenderer.time = 0;

        yield return new WaitForSeconds(0f);

        m_TrailRenderer.time = trailTime;
    }


    public bool CanSprint
    {
        get { return canSprint; }
        protected set { canSprint = value; }
    }
    public float SpeedEffect
    {
        get { return speedMultiplier; }
    }
    public float RotationEffect
    {
       get { return rotationMultiplier; }
    }

}
