using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPoolManager : MonoBehaviour {

    [SerializeField]
    string loadPath;

    [SerializeField]
    List<ItemPool> m_Pools = new List<ItemPool>();
    

    public static ItemPoolManager Instance { get; private set; }

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
    }
    void Start()
    {
        BuildPools();
    }
    public void BuildPools()
    {
        m_Pools.ForEach(p => p.ClearItems());

        ItemPrice[] loadedItems = Resources.LoadAll<ItemPrice>(loadPath);
        
        for(int i = 0; i < loadedItems.Length; i++)
        {
            List<PoolWeighting> poolWeights = loadedItems[i].Pools;
            for(int k = 0; k < poolWeights.Count; k++)
            {
                if(poolWeights[k].poolDefinition == ItemPoolDefinition.None)
                {
                    continue;
                }

                AddItem(loadedItems[i].gameObject, poolWeights[k].poolDefinition, poolWeights[k].weight);
            }
        }
    }


    public GameObject GetItem(ItemPoolDefinition definition, float luckBonus)
    {
        throw new NotImplementedException();
    }
    
    void AddItem(GameObject obj, ItemPoolDefinition definition, int weight)
    {
        ItemPool pool = m_Pools.Find(p => p.Definition == definition);

        if (pool == null)
            return;

        pool.AddItem(obj, weight);
    }



    void OnValidate()
    {
        ValidateItemPools();
    }
    void ValidateItemPools()
    {
        HashSet<ItemPoolDefinition> referenceSet = new HashSet<ItemPoolDefinition>((ItemPoolDefinition[])Enum.GetValues(typeof(ItemPoolDefinition)));
        referenceSet.Remove(ItemPoolDefinition.None);

        for (int i = 0; i < m_Pools.Count; i++)
        {
            if (!referenceSet.Contains(m_Pools[i].Definition))
            {
                m_Pools.RemoveAt(i);
                i--;
            }
            else
            {
                referenceSet.Remove(m_Pools[i].Definition);

                m_Pools[i].Validate();
            }
        }

        HashSet<ItemPoolDefinition>.Enumerator enumerator = referenceSet.GetEnumerator();

        while (enumerator.MoveNext())
        {
            m_Pools.Add(new ItemPool(enumerator.Current));
        }


        m_Pools.Sort(delegate (ItemPool a, ItemPool b)
        {
            return a.Definition < b.Definition ? -1 : 1;
        });
    }
}
