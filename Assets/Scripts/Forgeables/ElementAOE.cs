using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementAOE : AOETurret {

    [SerializeField]
    [EnumFlags]
    Attribute m_Attribute;

    [SerializeField]
    float amount;

    [SerializeField]
    float rateOfFire;

    [SerializeField]
    AnimationCurve fallOff = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    float timer = 0f;
    void Update()
    {

    }

    #region Accessors

    public float Amount
    {
        get { return amount; }
    }

    public float RateOfFire
    {
        get { return rateOfFire; }
    }

    #endregion

    protected override void OnValidate()
    {
        base.OnValidate();

        Utilities.ValidateCurve(fallOff, 0f, 1f, 0f, 1f);
    }
}
