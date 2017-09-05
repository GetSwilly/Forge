using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemPoolListDefinition
{
    [SerializeField]
    ListDefinitionName m_ListName;


    [SerializeField]
    List<Tuple_ItemPool_PoolWeightIdentifier> m_Pools = new List<Tuple_ItemPool_PoolWeightIdentifier>();


    public ItemPoolListDefinition(ListDefinitionName listName)
    {
        m_ListName = listName;
    }



    public GameObject GetItem(float luckBonus)
    {
        //Get Total Probability
        double totalProbability = 0;

        for (int i = 0; i < m_Pools.Count; i++)
        {
            totalProbability += GameManager.Instance == null ? 1f : GameManager.Instance.GetPoolWeight(m_Pools[i].Item2);
        }
         

        //Select a random value
        float pickValue = UnityEngine.Random.Range(0, (float)totalProbability) + (luckBonus * (float)totalProbability);

        for (int i = 0; i < m_Pools.Count; i++)
        {
            pickValue -= GameManager.Instance == null ? 1f : (float)GameManager.Instance.GetPoolWeight(m_Pools[i].Item2);

            if (pickValue <= 0f)
            {
                return m_Pools[i].Item1.GetItem(luckBonus);
            }
        }



        return null;
    }




    public ListDefinitionName ListName
    {
        get { return m_ListName; }
    }


    void SortItems()
    {
        if (GameManager.Instance == null)
            return;

        m_Pools.Sort((p1, p2) => GameManager.Instance.GetPoolWeight(p2.Item2).CompareTo(GameManager.Instance.GetPoolWeight(p1.Item2)));
    }

    public void Validate()
    {
        //if (m_Pools != null)
        //{
        //    for (int i = 0; i < m_Pools.Count; i++)
        //    {
        //        if (m_Pools[i].Item1 == null)
        //        {
        //            m_Pools.RemoveAt(i);
        //            i--;
        //        }
        //    }
        //}

        SortItems();
    }
}