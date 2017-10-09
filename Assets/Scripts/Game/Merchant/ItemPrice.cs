using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPrice : MonoBehaviour {

    [SerializeField]
    int m_Cost;
    
    public void Charge()
    {
        throw new System.NotImplementedException();
    }

    public int Cost
    {
        get { return m_Cost; }
        set { m_Cost = Mathf.Clamp(value, 0, value); }
    }


    void OnValidate()
    {
        Cost = Cost;
    }
}
