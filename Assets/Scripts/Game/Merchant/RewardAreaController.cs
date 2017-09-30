using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(LootDrafter))]
[RequireComponent(typeof(CustomLayout))]
public class RewardAreaController : MonoBehaviour {

    enum DeactivateTrigger { Acquisition, Removal}

    [SerializeField]
    DeactivateTrigger m_Trigger = DeactivateTrigger.Removal;

    [SerializeField]
    ListDefinitionName m_ListDefinition;


    [SerializeField]
    ItemPool customLootTable;

    [SerializeField]
    bool spawnOnAwake = true;


    int numUses = 1;

    List<GameObject> trackedObjects = new List<GameObject>();

    LootDrafter m_Drafter;
    CustomLayout m_Layout;

    void Awake()
    {
        m_Drafter = GetComponent<LootDrafter>();
        m_Layout = GetComponent<CustomLayout>();
    }
    void OnEnable()
    {
        PlayerController pController = GameManager.Instance.PlayerController;
        SpawnRewards(pController == null ? 0f : pController.GetCurrentStatLevel(StatType.Luck));
    }

    public void SpawnRewards(float luckBonus)
    {
        ClearRewards();

        List<GameObject> newRewards = m_Drafter.GetLoot(luckBonus);
        for(int i = 0; i < newRewards.Count; i++)
        {
            newRewards[i].transform.SetParent(this.transform);
            newRewards[i].SetActive(true);
        }

        m_Layout.SetImmediateLayout();
        TrackRewards();
    }
    void TrackRewards()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            IAcquirableObject iObj = transform.GetChild(i).GetComponent<IAcquirableObject>();

            if (iObj == null)
                continue;

            iObj.ObjectAcquired += TrackedObjectUsed;
            trackedObjects.Add(iObj.Object);
        }
    }
    public void ClearRewards()
    {
       while(transform.childCount > 0)
        {
            Destroy(transform.GetChild(0));
        }

        trackedObjects.Clear();
    }
    




    void SubtractUse() { SubtractUse(1); }
    void SubtractUse(int amt)
    {
        numUses -= amt;

        if(numUses <= 0)
        {
            DeactivateArea();
        }
    }
    void DeactivateArea()
    {
        for(int i = 0; i < trackedObjects.Count; i++)
        {
            trackedObjects[i].GetComponent<IAcquirableObject>().Drop();
        }
    }



    public int NumberOfUses
    {
        get { return numUses; }
        set { numUses = value; }
    }

    private void TrackedObjectUsed(object sender, System.EventArgs e)
    {
        if (m_Trigger != DeactivateTrigger.Acquisition)
            return;

        IAcquirableObject iObj = sender as IAcquirableObject;

        if (iObj == null)
            return;

        if (trackedObjects.Contains(iObj.Object))
        {
            trackedObjects.Remove(iObj.Object);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger || !trackedObjects.Contains(other.gameObject))
            return;


        if(m_Trigger == DeactivateTrigger.Removal)
        {
            trackedObjects.Remove(other.gameObject);
        }

    }
}
