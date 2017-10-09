using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cost : PropertyAttribute {

    [SerializeField]
    CurrencyType m_Type;

    [SerializeField]
    int m_Value;

    [SerializeField]
    StatType m_StatType;


    public Cost(CurrencyType _currencyType)
    {
        Type = _currencyType;
    }
    public Cost(CurrencyType _currencyType, StatType _statType)
    {
        Type = _currencyType;
        StatType = _statType;
    }



    #region Accessors

    public CurrencyType Type
    {
        get { return m_Type; }
        set { m_Type = value; }
    }
    public int Value
    {
        get { return m_Value; }
        set { m_Value = value; }
    }
    public StatType StatType
    {
        get { return m_StatType; }
        set { m_StatType = value; }
    }

    #endregion

    public override string ToString()
    {
        string str = "";

        switch (Type)
        {
            case CurrencyType.StatLevel:
                str = Type + " : " + StatType +  " : " + Value;
                break;
            default:
                str = Type + " : " + Value;
                break;
        }

        return str;
    }

}
