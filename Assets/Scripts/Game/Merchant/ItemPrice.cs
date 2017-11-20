using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPrice : MonoBehaviour {

    [SerializeField]
    List<PoolWeighting> m_Pools = new List<PoolWeighting>();

    [SerializeField]
    int creditValue;
    

    public List<PoolWeighting> Pools
    {
        get { return m_Pools; }
    }

    public int CreditValue
    {
        get { return creditValue; }
        set { creditValue = Mathf.Clamp(value, 0, value); }
    }


    void OnValidate()
    {
        CreditValue = CreditValue;

        ValidatePools();
    }

    void ValidatePools()
    {
        HashSet<ItemPoolDefinition> referenceSet = new HashSet<ItemPoolDefinition>();
        
        for(int i = 0; i < m_Pools.Count; i++)
        {
            if (referenceSet.Contains(m_Pools[i].poolDefinition))
            {
                m_Pools.RemoveAt(i);
                i--;
            }
            else
            {
                referenceSet.Add(m_Pools[i].poolDefinition);
                m_Pools[i].Validate();
            }
        }
    }
}

[System.Serializable]
public class PoolWeighting
{
    [SerializeField]
    public ItemPoolDefinition poolDefinition;

    [SerializeField]
    public int weight;

    [SerializeField]
    public Rank rank;

    public void Validate()
    {
        weight = Mathf.Clamp(weight, 0, weight);
    }
}
