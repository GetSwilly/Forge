using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPrice : MonoBehaviour {

    [SerializeField]
    int m_Value;
    
    public void Charge()
    {
        throw new System.NotImplementedException();
    }

    public int Value
    {
        get { return m_Value; }
        set { m_Value = Mathf.Clamp(value, 0, value); }
    }


    void OnValidate()
    {
        Value = Value;
    }
}
