using UnityEngine;
using System.Collections;
using System;


[RequireComponent(typeof(Light))]
public class Flashlight : Tool
{
    static readonly float DELAY_TIME = 1f;

    [SerializeField]
    [Range(0f, 360f)]
    float m_Range = 10f;

    [SerializeField]
    [Range(1f, 179f)]
    float m_Angle = 30f;

    bool isUsable = true;
    Light m_Light;

    [SerializeField]
    bool isActive = false;

    public override void Awake()
    {
        base.Awake();

        m_Light = GetComponent<Light>();
    }


    public override void UpdateStat(Stat _stat)
    {
        Range = Range;
        Angle = Angle;
    }



    public override void ActivatePrimary()
    {
        if (!isUsable)
            return;


        IsActive = !IsActive;

        AlertPrimaryActivation();

        StartCoroutine(DelayUsability());
    }
    

    public override void DisableEffects() { }
    
    public override float GetPercentage()
    {
        return 0f;
    }
    

    public override void DeactivatePrimary()
    {
    }

    public override bool CanActivatePrimary()
    {
        return isUsable;
    }

    public override void ActivateSecondary()
    {
    }

    public override void DeactivateSecondary()
    {
    }

    public override bool CanActivateSecondary()
    {
        return true;
    }

    public override void ActivateTertiary()
    {
    }

    public override void DeactivateTertiary()
    {
    }

    public override bool CanActivateTertiary()
    {
        return false;
    }

    public override void EnableEffects()
    {
        throw new NotImplementedException();
    }
   

    IEnumerator DelayUsability()
    {
        isUsable = false;

        yield return new WaitForSeconds(DELAY_TIME);

        isUsable = true;
    }


    public bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;

            Light _light = m_Light == null ? GetComponent<Light>() : m_Light;
            _light.enabled = isActive;
        }
    }
    public float Range
    {
        get { return m_Range; }
        protected set
        {
            m_Range = value;


            Light _light = m_Light == null ? GetComponent<Light>() : m_Light;
            _light.range = Mathf.Clamp(m_Range * (GetStatValue(StatType.Damage) + 1), 0f, 360f);
        }
    }
    public float Angle
    {
        get { return m_Angle; }
        protected set
        {
            m_Angle = value;

            Light _light = m_Light == null ? GetComponent<Light>() : m_Light;
            _light.spotAngle = Mathf.Clamp(m_Angle * (GetStatValue(StatType.Damage) + 1),1f,179f);
        }
    }


   void OnValidate()
    {
        Range = Range;
        Angle = Angle;
        IsActive = IsActive;
    }
}
