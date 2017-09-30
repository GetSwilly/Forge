using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Merchandise {

    [SerializeField]
    GameObject m_Object;

    [SerializeField]
    Cost activeCost;

    ItemPrice m_Price;

    public Merchandise(GameObject obj, ItemPrice _price, CurrencyType _currency, StatType _stat)
    {
        m_Object = obj;
        m_Price = _price;

        activeCost = m_Price.GetCost(_currency, _stat);
    }


    public void SetCost(CurrencyType _currency, StatType _stat)
    {
        ActiveCost = Price.GetCost(_currency, _stat);
    }



    public string Name
    {
        get { return m_Object.name; }
    }
    public GameObject Object
    {
        get { return m_Object; }
        set { m_Object = value; }
    }
    public Cost ActiveCost
    {
        get { return activeCost; }
        set { activeCost = value; }
    }
    public ItemPrice Price
    {
        get { return m_Price; }
    }
}
