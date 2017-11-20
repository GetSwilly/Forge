using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemPool
{

    [SerializeField]
    string m_PoolName = "";

    [SerializeField]
    int nullWeight;

    class Item
    {
        GameObject m_Object;
        int weight;

        public Item(GameObject obj, int weight)
        {
            m_Object = obj;
            Weight = weight;
        }

        public GameObject Object
        {
            get { return m_Object; }
        }
        public int Weight
        {
            get { return weight; }
            set { weight = Mathf.Clamp(value, 0, value); }
        }
    }

    List<Item> m_Items = new List<Item>();


    //[SerializeField]
    //bool allowRepeatPulls = true;


    //[SerializeField]
    //bool autoFill = true;


    ItemPoolDefinition definition;


    public ItemPool(ItemPoolDefinition definition)
    {
        this.definition = definition;
        m_PoolName = Definition.ToString();
    }



    public GameObject GetItem()
    {
        return GetItem(0f);
    }
    public GameObject GetItem(float luckBonus)
    {
        GameObject lootedObj = null;

        
        //Get Total Probability
        float totalProbability = 0;

        for (int i = 0; i < m_Items.Count; i++)
        {
            totalProbability += m_Items[i].Weight;
        }




        //Select a random value
        float pickValue = UnityEngine.Random.Range(0, totalProbability) + (luckBonus * totalProbability);

        for (int i = 0; i < m_Items.Count; i++)
        {
            pickValue -= m_Items[i].Weight;

            if (pickValue <= 0)
            {
                lootedObj = m_Items[i].Object;
                break;
            }
        }


        if (lootedObj == null)
            return null;


        GameObject retObj = GameObject.Instantiate(lootedObj);

        //GameObject genObj = GameObject.Find("Generated Objects");
        //retObj.transform.parent = (genObj != null) ? genObj.transform : null;


        return retObj;
    }


    public void AddItem(GameObject obj, int weight)
    {
        if (weight <= 0 || m_Items.Exists(i => i.Object.Equals(obj)))
        {
            return;
        }

        m_Items.Add(new Item(obj, weight));
    }

    public void ClearItems()
    {
        m_Items.Clear();
    }


    public ItemPoolDefinition Definition
    {
        get { return definition; }
    }
    public int NullWeight
    {
        get { return nullWeight; }
       private set { nullWeight = Mathf.Clamp(value, 0, value); }
    }


    void SortItems()
    {
        if (m_Items == null)
            return;

        m_Items.Sort((i1, i2) => i1 == null ? 1 : (i2 == null ? -1 : i2.Weight.CompareTo(i1.Weight)));
    }


    public void Validate()
    {
        m_PoolName = Definition.ToString();

        NullWeight = NullWeight;

        SortItems();
    }

}

