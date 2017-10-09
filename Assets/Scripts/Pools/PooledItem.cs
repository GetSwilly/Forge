using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledItem : MonoBehaviour {
    
    [SerializeField]
    float m_Weight;

    [SerializeField]
    Rank m_Rank;
    

   
    public float Weight
    {
        get { return m_Weight; }
        private set { m_Weight = Mathf.Clamp(value, 0, value); }
    }
    public Rank Rank
    {
        get { return m_Rank; }
    }


    void OnValidate()
    {
        Weight = Weight;
    }
}
