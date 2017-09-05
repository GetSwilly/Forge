using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPrice : MonoBehaviour {

    [SerializeField]
    List<Cost> m_Costs = new List<Cost>();



    public Cost GetCost(CurrencyType _currency, StatType _stat)
    {
        for(int i = 0; i < m_Costs.Count; i++)
        {
            if(m_Costs[i].Currency == _currency)
            {
                if(_currency == CurrencyType.StatLevel)
                {
                    if(m_Costs[i].StatType == _stat)
                    {
                        return m_Costs[i];
                    }
                }
                else
                {
                    return m_Costs[i];
                }
            }
        }

        return null;
    }



    void FillCostTypes()
    {
        HashSet<CurrencyType> currencySet = new HashSet<CurrencyType>(Enum.GetValues(typeof(CurrencyType)) as CurrencyType[]);
        HashSet<StatType> statSet = new HashSet<StatType>(Enum.GetValues(typeof(StatType)) as StatType[]);


        for (int i = 0; i < m_Costs.Count; i++)
        {
            if (m_Costs[i].Currency == CurrencyType.StatLevel)
            {
                if (!statSet.Contains(m_Costs[i].StatType))
                {
                    m_Costs.RemoveAt(i);
                    i--;
                    continue;
                }
                else
                {
                    statSet.Remove(m_Costs[i].StatType);
                }
            }
            else
            {
                if (!currencySet.Contains(m_Costs[i].Currency))
                {
                    m_Costs.RemoveAt(i);
                    i--;
                    continue;
                }
                else
                {
                    currencySet.Remove(m_Costs[i].Currency);
                }
            }
        }

        HashSet<CurrencyType>.Enumerator currencyEnumerator = currencySet.GetEnumerator();
        while (currencyEnumerator.MoveNext())
        {
            if (currencyEnumerator.Current == CurrencyType.StatLevel)
                continue;

            m_Costs.Add(new Cost(currencyEnumerator.Current));
        }



        HashSet<StatType>.Enumerator statEnumerator = statSet.GetEnumerator();
        while (statEnumerator.MoveNext())
        {
            m_Costs.Add(new Cost(currencyEnumerator.Current, statEnumerator.Current));
        }
    }


    void OnValidate()
    {
        FillCostTypes();
    }
}
