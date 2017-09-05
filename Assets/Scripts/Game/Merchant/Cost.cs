using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cost : PropertyAttribute {

    [SerializeField]
    CurrencyType m_Currency;

    [SerializeField]
    int m_Value;

    [SerializeField]
    StatType m_StatType;

    //[SerializeField]
    //int m_LevelPointCost;

    //[SerializeField]
    //int m_HealthCost;

    //[SerializeField] int m_ExperienceCost;

    //[SerializeField]
    //List<Tuple_StatTypeInt> m_StatLevelCost = new List<Tuple_StatTypeInt>();


    public Cost(CurrencyType _currencyType)
    {
        Currency = _currencyType;
    }
    public Cost(CurrencyType _currencyType, StatType _statType)
    {
        Currency = _currencyType;
        StatType = _statType;
    }




    public CurrencyType Currency
    {
        get { return m_Currency; }
        set { m_Currency = value; }
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


    //public int LevelPointCost
    //{
    //    get { return m_LevelPointCost; }
    //    set { m_LevelPointCost = value; }
    //}
    //public int HealthCost
    //{
    //    get { return m_HealthCost; }
    //    set { m_HealthCost = value; }
    //}
    //public int ExperienceCost
    //{
    //    get { return m_ExperienceCost; }
    //    set { m_ExperienceCost = value; }
    //}
    //public List<Tuple_StatTypeInt> StatLevelCost
    //{
    //    get { return m_StatLevelCost; }
    //    set { m_StatLevelCost = value; }
    //}





    //public void Validate()
    //{
    //    HashSet<StatType> encounteredSet = new HashSet<StatType>();
    //    for(int i = 0; i < m_StatLevelCost.Count; i++)
    //    {
    //        if (encounteredSet.Contains(m_StatLevelCost[i].Item1))
    //        {
    //            m_StatLevelCost.RemoveAt(i);
    //            i--;
    //            continue;
    //        }
    //        else
    //        {
    //            encounteredSet.Add(m_StatLevelCost[i].Item1);
    //        }
    //    }
    //}
}
