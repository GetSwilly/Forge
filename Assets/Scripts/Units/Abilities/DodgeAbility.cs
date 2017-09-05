using UnityEngine;
using System.Collections;
using System;

public class DodgeAbility : Ability {

    [Space(5)]
    [Header("Dodge")]
    [Space(5)]


    [Tooltip("Amount of time dodge will be active for")]
    [SerializeField]
    float dodgeTime = 1;

    [Tooltip("Speed of active dodge")]
    [SerializeField]
    float dodgeSpeed = 1;
    

    Rigidbody m_Rigidbody;
    UnitController m_Controller;




    public override void Initialize(Transform _transform)
    {
        if (_transform == null)
            this.enabled = false;

        m_Rigidbody = _transform.GetComponent<Rigidbody>();
        m_Controller = _transform.GetComponent<UnitController>();

        if (m_Rigidbody == null || m_Controller == null)
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

        Vector2 inputVector = new Vector2(h, v);//.normalized * dodgeSpeed;

        StartCoroutine(DodgeActivity(inputVector));
        SetCharge(0f);
    }
    public override void DeactivateAbility()
    {
        if (!IsAbilityActive)
            return;


        Vector3 newVel = Vector3.zero;
        newVel.y = m_Rigidbody.velocity.y;
        m_Rigidbody.velocity = newVel;

        m_Controller.enabled = true;

        SetCharge(0f);

        IsAbilityActive = false;
    }



    IEnumerator DodgeActivity(Vector2 inputVector)
    {
        IsAbilityActive = true;

        m_Controller.enabled = false;


        inputVector = inputVector.normalized * dodgeSpeed;

        Vector3 dodgeVector = new Vector3(inputVector.x, 0, inputVector.y);

        float timer = 0;
        while (timer < dodgeTime)
        {

            timer += Time.deltaTime;

            dodgeVector.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = dodgeVector;


            yield return null;
        }

        DeactivateAbility();


        IsAbilityActive = false;

    }
	
}
