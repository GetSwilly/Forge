using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPrice : MonoBehaviour {

    [SerializeField]
    List<Cost> m_Costs = new List<Cost>();
    
    public void Charge()
    {
        throw new System.NotImplementedException();
    }

    public List<Cost> Costs
    {
        get { return m_Costs; }
    }
}
