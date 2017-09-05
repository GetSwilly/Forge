using UnityEngine;
using System.Collections;
using System;

public class Grapple : Tool {

    static readonly float HOOK_ACCEPTABLE_PROXIMITY = 0.5f;

    [SerializeField]
    float hookMovementForce;

    [SerializeField]
    float extensionRange;

    [SerializeField]
    float pullForce;

    [SerializeField]
    float maxTime;

    [SerializeField]
    GrappleHook m_Hook;

    Transform hookTransform;
    Rigidbody hookRigid;

    public override void Awake()
    {
        base.Awake();

        hookTransform = m_Hook.transform;
        hookRigid = m_Hook.GetComponent<Rigidbody>();
    }
    void Start()
    {
        m_Hook.OnHook += HookedEvent;
    }

    public override void UpdateStat(Stat _stat) { }



    IEnumerator ExtendHook()
    {
        m_Hook.transform.parent = null;
        m_Hook.CanHook = true;

       
        hookRigid.velocity = Vector3.zero;

        Vector3 previousPosition = hookTransform.position;
        Vector3 extendDir = m_Transform.forward;
        float timer = maxTime;
        float travelDistance = 0f;

        while(timer > 0 && travelDistance <= extensionRange)
        {
            yield return null;

            timer -= Time.deltaTime;

            travelDistance += Vector3.Distance(hookTransform.position, previousPosition);
            previousPosition = hookTransform.position;
            
            hookRigid.AddForce(extendDir * hookMovementForce * Time.deltaTime, ForceMode.Acceleration);
        }

        StartCoroutine(PullHook(false));
    }
    IEnumerator PullHook(bool isHooked)
    {
        hookRigid.velocity = Vector3.zero;

        Rigidbody attachedRigid = isHooked ? m_Hook.HookedTransform.GetComponent<Rigidbody>() : null;
        

        if (isHooked)
        {
            hookTransform.parent = m_Hook.HookedTransform;
        }

        m_Hook.CanHook = false;

        float timer = maxTime;

        while(timer > 0 && Vector3.Distance(hookTransform.position, m_Transform.position) > HOOK_ACCEPTABLE_PROXIMITY)
        {
            yield return null;

            timer -= Time.deltaTime;

            Vector3 pullDir = hookTransform.position - m_Transform.position;
            if (isHooked)
            {
                attachedRigid.AddForce(pullDir.normalized * pullForce * Time.deltaTime, ForceMode.Force);
            }
            else
            {
                hookRigid.AddForce(pullDir.normalized * hookMovementForce * Time.deltaTime, ForceMode.Acceleration);
            }
        }

        hookTransform.position = m_Transform.position;
        hookTransform.rotation = m_Transform.rotation;
        hookTransform.parent = m_Transform;
    }


    void HookedEvent()
    {
        StopAllCoroutines();
        StartCoroutine(PullHook(true));
    }


    public override void ActivatePrimary()
    {
        throw new NotImplementedException();
    }

    public override void DeactivatePrimary()
    {
        m_Hook.CanHook = false;
        throw new NotImplementedException();
    }

    public override bool CanActivatePrimary()
    {
        throw new NotImplementedException();
    }

    public override void ActivateSecondary()
    {
        throw new NotImplementedException();
    }

    public override void DeactivateSecondary()
    {
        throw new NotImplementedException();
    }

    public override bool CanActivateSecondary()
    {
        throw new NotImplementedException();
    }

    public override void ActivateTertiary()
    {
        throw new NotImplementedException();
    }

    public override void DeactivateTertiary()
    {
        throw new NotImplementedException();
    }

    public override bool CanActivateTertiary()
    {
        throw new NotImplementedException();
    }

    public override void EnableEffects()
    {
        throw new NotImplementedException();
    }

    public override void DisableEffects()
    {
        throw new NotImplementedException();
    }

    public override float GetPercentage()
    {
        throw new NotImplementedException();
    }
}
