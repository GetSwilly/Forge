using UnityEngine;
using System.Collections;
using System;

public class Chest : InteractableObject
{
    static readonly float TEMP_BONUS = 0f;
    static readonly float SPAWN_DELAY = 0.25f;

    [SerializeField]
    ItemPoolDefinition m_ListDefinition;


    [SerializeField]
    bool useCustomItemPool = false;


    [SerializeField]
    ItemPool customItemPool;
    


    [System.Serializable]
    struct SpawnStruct
    {
        public Vector3 localOffset;
        public float launchPower;
    }

    [SerializeField]
    SpawnStruct spawnVariables;

    

    [SerializeField]
    AnimationCurve numberOfRewardsCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    

    [SerializeField]
    float rechargeTime = .5f;
    
    bool isUsable = true;



    public override bool Interact(PlayerController player)
    {
        if (!isUsable)
            return false;


        StartCoroutine(DropRewards(player.GetCurrentStatLevel(StatType.Luck)));
        StartCoroutine(RechargeChest());
        
        OnUseTrigger();

        return true;
    }


    public override void Drop()
    {

    }





    IEnumerator RechargeChest()
    {
        isUsable = false;

        yield return new WaitForSeconds(rechargeTime);

        isUsable = true;
    }



    IEnumerator DropRewards(int luckLevel)
    {
        float luckBonus = TEMP_BONUS;

        int _num = (int)numberOfRewardsCurve.Evaluate(UnityEngine.Random.value);

        for (int i = 0; i < _num; i++)
        {
            if (GameManager.Instance == null)
                continue;



            GameObject newDrop = null;
            

            if (useCustomItemPool)
            {
                newDrop = customItemPool.GetItem(luckBonus);
            }
            else
            {
                newDrop = GameManager.Instance.GetItem(m_ListDefinition, luckBonus);
            }

           
            if (newDrop == null)
                continue;




            Vector3 newPos = transform.TransformPoint(spawnVariables.localOffset);

            Vector3 bounds = Utilities.CalculateObjectBounds(newDrop, false);
            newPos.y += bounds.y / 2f;

            newDrop.transform.position = newPos;
            newDrop.transform.rotation = Quaternion.identity; 
            newDrop.SetActive(true);


            Rigidbody _rigidbody = newDrop.GetComponent<Rigidbody>();

            if (_rigidbody != null)
            {
                Vector3 forceVector = UnityEngine.Random.onUnitSphere * spawnVariables.launchPower;
                forceVector.y = Mathf.Abs(forceVector.y);

                _rigidbody.velocity = Vector3.zero;
                _rigidbody.AddForce(forceVector);
            }

            yield return new WaitForSeconds(SPAWN_DELAY);

        }
    }

   




    public override bool IsUsableOutsideFOV
    {
        get { return false; }
    }
}
