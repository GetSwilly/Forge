using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomLayout))]
public class LootDrafter : MonoBehaviour {

    [SerializeField]
    ItemPoolDefinition m_LootList;

    [SerializeField]
    bool useCustomItemPool;

    [SerializeField]
   ItemPool m_CustomItemPool;



    [SerializeField]
    AnimationCurve numberOfRewardsCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);


    public List<GameObject> GetLoot() { return GetLoot(0f); }
    public List<GameObject> GetLoot(float luckBonus)
    {
        int _num = (int)numberOfRewardsCurve.Evaluate(UnityEngine.Random.value);

        List<GameObject> lootList = new List<GameObject>();


        for (int i = 0; i < _num; i++)
        {
            GameObject newDrop = null;


            if (useCustomItemPool)
            {
                newDrop = m_CustomItemPool.GetItem(luckBonus);
            }
            else
            {
                newDrop = GameManager.Instance.GetItem(m_LootList, luckBonus);
            }


            if (newDrop == null)
                continue;


            lootList.Add(newDrop);
        }


        return lootList;
    }


}
