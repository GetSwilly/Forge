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
    }
    public Rank Rank
    {
        get { return m_Rank; }
    }


    void OnValidate()
    {
        if (Weight < 0)
            m_Weight = 0;
    }
}
