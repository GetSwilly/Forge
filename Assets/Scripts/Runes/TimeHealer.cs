using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeHealer : Rune
{

    [Tooltip("Amount of healing to apply every second")]
    [SerializeField]
    AnimationCurve healingCurve = AnimationCurve.Linear(0f, 0f, 100f, 0f);

    Health ownerHealth;


    void Update()
    {
        if (WeatherSystem.Instance != null && Owner != null)
        {
            float time = WeatherSystem.Instance.CurrentTime;

            if (ownerHealth != null)
            {
                ownerHealth.HealthArithmetic(healingCurve.Evaluate(time), false, m_Transform);
            }
        }
    }


    public override void Initialize(UnitController _unit)
    {
        base.Initialize(_unit);

        if (Owner != null)
        {
            ownerHealth = Owner.GetComponent<Health>();
        }
    }
    public override void Terminate()
    {
        base.Terminate();
        ownerHealth = null;
    }

    void OnValidate()
    {
        Utilities.ValidateCurve_Times(healingCurve, 0f, 100f);
    }
}
