using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPool : MonoBehaviour {
    
    public enum PoolType { Pools, Items }

    [SerializeField]
    public PoolType m_PoolType = PoolType.Items;

    [SerializeField]
    string m_PoolName = "";
    

    [SerializeField]
    List<Tuple_ItemPool_PoolWeightIdentifier> m_Pools = new List<Tuple_ItemPool_PoolWeightIdentifier>();

    [SerializeField]
    List<PooledItem> m_Items = new List<PooledItem>();
   
   
    [SerializeField]
    bool allowRepeatPulls = true;


    [SerializeField]
    bool autoFill = true;

    [SerializeField]
    Rank m_Rank;

    [SerializeField]
    List<string> resourcePaths = new List<string>();

    
    

    public void Awake()
    {
        ValidateItems(false);
        ValidatePools(false);

        if (Type == PoolType.Items && autoFill)
        {
            LoadItems();
        }
    }

    public void LoadItems()
    {
        m_Items.Clear();

        for (int k = 0; k < resourcePaths.Count; k++)
        {
            PooledItem[] loadedItems = Resources.LoadAll<PooledItem>(resourcePaths[k]);
            for (int i = 0; i < loadedItems.Length; i++)
            {
                if (loadedItems[i].Rank == Rank)
                {
                    AddItem(loadedItems[i]);
                }
            }
        }
    }





    public GameObject GetItem()
    {
        return GetItem(0f);
    }
    public GameObject GetItem(float luckBonus)
    {
        return GetItem(luckBonus, allowRepeatPulls);
    }
    public GameObject GetItem(float luckBonus, bool shouldRepeat)
    {
        if (m_PoolType == PoolType.Pools && m_Pools.Count > 0)
        {
            //Get Total Probability
            double totalProbability = 0;

            for (int i = 0; i < m_Pools.Count; i++)
            {
                totalProbability += GameManager.Instance == null ? 1 : GameManager.Instance.GetPoolWeight(m_Pools[i].Item2); 
        }



            //Select a random value
            double pickValue = UnityEngine.Random.Range(0, (float)totalProbability) + (luckBonus * totalProbability);

            for (int i = 0; i < m_Pools.Count; i++)
            {
                pickValue -= GameManager.Instance == null ? 1 : GameManager.Instance.GetPoolWeight(m_Pools[i].Item2);

                if (pickValue <= 0)
                {
                    return m_Pools[i].Item1.GetItem(luckBonus);
                }
            }


            return null;
        }
        else if(m_PoolType == PoolType.Items)
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
                    lootedObj = m_Items[i].gameObject;
                    break;
                }
            }


            if (lootedObj == null)
                return null;


            GameObject retObj = Instantiate(lootedObj) as GameObject;

            GameObject genObj = GameObject.Find("Generated Objects");
            retObj.transform.parent = (genObj != null) ? genObj.transform : null;


            return retObj;
        }

        return null;
    }


    public void AddItem(PooledItem item)
    {
        if (m_Items.Contains(item))
            return;

        m_Items.Add(item);
    }
    public void AddItem(string name, GameObject obj, float prob)
    {
        if (obj == null)
            return;

        throw new NotImplementedException();
    }




   
    public PoolType Type
    {
        get { return m_PoolType; }
    }
    public Rank Rank
    {
        get { return m_Rank; }
    }
    public bool AutoFill
    {
        get { return autoFill; }
    }
 





    void ValidateItems(bool leaveFiller)
    {
        HashSet<GameObject> objectSet = new HashSet<GameObject>();

        for (int i = 0; i < m_Items.Count; i++)
        {
            //Allow possibility of null Item return
            if (m_Items[i] == null && i != m_Items.Count - 1)
            {
                continue;
            }

            if (m_Items[i] == null || objectSet.Contains(m_Items[i].gameObject))
            {
                if(leaveFiller && i == m_Items.Count - 1)
                {
                    m_Items[i] = null;
                    continue;
                }

                m_Items.RemoveAt(i);
                i--;
            }
            else
            {
                objectSet.Add(m_Items[i].gameObject);
            }
        }

        //HashSet<string> pathSet = new HashSet<string>();
        //for(int i = 0; i < resourcePaths.Count; i++)
        //{
        //    if (pathSet.Contains(resourcePaths[i]))
        //    {
        //        resourcePaths.RemoveAt(i);
        //        i--;
        //    }
        //    else;
        //    {
        //        pathSet.Add(resourcePaths[i]);
        //    }
        //}
    }
    void ValidatePools(bool leaveFiller)
    {
        HashSet<ItemPool> objectSet = new HashSet<ItemPool>();

        for (int i = 0; i < m_Pools.Count; i++)
        {
            if (m_Pools[i] == null || objectSet.Contains(m_Pools[i].Item1))
            {
                if (leaveFiller && i == m_Pools.Count - 1)
                {
                    m_Pools[i] = null;
                    continue;
                }

                m_Pools.RemoveAt(i);
                i--;
            }
            else
            {
                objectSet.Add(m_Pools[i].Item1);
            }
        }
    }
    void SortItems()
    {
        m_Items.Sort((i1, i2) => i1 == null ? 1 : (i2 == null ? -1 : i2.Weight.CompareTo(i1.Weight)));
    }


    void OnValidate()
    {
        ValidateItems(true);
        ValidatePools(true);
        SortItems();
    }

}

