using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandheldShield : HandheldItem
{
    [SerializeField]
    GameObject m_Shield;

    [SerializeField]
    float rechargeRate = 0f;

    Health shieldHealth;
    Team shieldTeam;

    protected override void Awake()
    {
        base.Awake();

        m_Shield.SetActive(true);
        m_Shield.SetActive(false);
        shieldHealth = m_Shield.GetComponent<Health>();
        shieldTeam = m_Shield.GetComponent<Team>();
    }
    void Start()
    {
        shieldHealth.OnKilled += DisableShield;
    }

    public override void Initialize(Transform owner, Team team)
    {
        base.Initialize(owner, team);

        shieldTeam.Copy(team);
        m_Shield.SetActive(false);
    }
    IEnumerator RechargeRoutine()
    {
        while (true)
        {
            yield return null;
            if (!m_Shield.activeInHierarchy)
            {
                shieldHealth.HealthArithmetic(RechargeRate * Time.deltaTime, false, m_Transform);
            }
        }
    }

    void DisableShield(Health h)
    {
        m_Shield.SetActive(false);
    }

    #region Primary

    public override bool CanActivatePrimary()
    {
        return shieldHealth.IsAlive;
    }
    public override void ActivatePrimary()
    {
        m_Shield.SetActive(true);
    }
    public override void DeactivatePrimary()
    {
        m_Shield.SetActive(false);
    }

    #endregion

    #region Secondary

    public override bool CanActivateSecondary()
    {
        return false;
    }
    public override void ActivateSecondary()
    {
       
    }
    public override void DeactivateSecondary()
    {
      
    }

    #endregion


    #region Tertiary

    public override bool CanActivateTertiary()
    {
        return false;
    }
    public override void ActivateTertiary()
    {
       
    }
    public override void DeactivateTertiary()
    {

    }

    #endregion


    public override float GetPercentage()
    {
       return shieldHealth.HealthPercentage;
    }


    public float RechargeRate
    {
        get { return rechargeRate; }
        private set { rechargeRate = Mathf.Clamp(value, 0f, value); }
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        RechargeRate = RechargeRate;
    }
}
