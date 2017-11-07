using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAbility : Ability {


    [Header("Dash")]

    [Tooltip("Amount of time dash will be active for")]
    [SerializeField]
    float dashTime = 1;

    [Tooltip("Speed of active dash")]
    [SerializeField]
    float dashSpeed = 1;

    bool isDashing = false;

    Transform unitTransform;
    MovementController unitMovement;
    UnitController unitController;


    public override void Initialize(Transform _transform)
    {
        if (_transform == null)
            this.enabled = false;

        unitTransform = _transform;
        unitMovement = _transform.GetComponent<MovementController>();
        unitController = _transform.GetComponent<UnitController>();

        if (unitMovement == null || unitController == null)
            this.enabled = false;
    }
    public override void Terminate()
    {
        DeactivateAbility();
    }


    public override void ActivateAbility()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector2 inputVector = new Vector2(h, v);

        StartCoroutine(DashActivity(inputVector));
        SetCharge(0f);
    }
    public override void DeactivateAbility()
    {
        if (!IsAbilityActive || isDashing)
            return;

        StopAllCoroutines();

        unitController.enabled = true;

        SetCharge(0f);

        IsAbilityActive = false;
    }



    IEnumerator DashActivity(Vector2 inputVector)
    {
        IsAbilityActive = true;
        isDashing = true;

        unitController.enabled = false;


        Vector3 dashVector = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        if (showDebug)
        {
            Debug.DrawLine(unitTransform.position, unitTransform.position + (dashVector * DashSpeed), Color.magenta, 2f);
        }

        PlaySound(m_Sound);

        float timer = 0;
        while (timer < DashTime)
        {
            yield return null;
            timer += Time.deltaTime;

            unitMovement.Move(unitTransform.position, dashVector, true, DashSpeed);
        }

        isDashing = false;
        DeactivateAbility();
    }


    protected float DashTime
    {
        get { return dashTime; }
        private set { dashTime = Mathf.Clamp(value, 0f, value); }
    }
    protected float DashSpeed
    {
        get { return dashSpeed; }
        private set { dashSpeed = Mathf.Clamp(value, 0f, value); }
    }


    protected override void OnValidate()
    {
        base.OnValidate();

        DashTime = DashTime;
        DashSpeed = DashSpeed;
    }
}
