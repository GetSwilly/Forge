using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SacrificialAltar : InteractableObject
{
    static readonly float TEMP_BONUS = 0f;

    static readonly float SPAWN_DELAY = 0.25f;
    //static readonly float REWARD_START_HEIGHT_OFFSET = 2f;

    [Space(10)]
    [Header("Standard")]
    [Space(5)]


    [SerializeField]
    ListDefinitionName m_StandardListDefinition;

    [SerializeField]
    SpawnStruct standardSpawn;

    [SerializeField]
    bool useCustomStandard = false;

    [SerializeField]
    ItemPool standardCustomItemPool;

    [SerializeField]
    AnimationCurve rewardCountCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);


    //---------------------------------------------------------

    [Space(10)]
    [Header("Summon")]
    [Space(5)]



    [SerializeField]
    [Range(0f, 1f)]
    float summonChance = 0f;

    [SerializeField]
    ListDefinitionName m_RewardListDefinition;

    [SerializeField]
    bool useCustomExtinction = false;

    [SerializeField]
    ItemPool extinctionCustomItemPool;


    [SerializeField]
    SpawnStruct summonSpawn;

    [SerializeField]
    ItemPool summonPool;

    [SerializeField]
    AnimationCurve summonCountCurve = AnimationCurve.Linear(0f, 0f, 1f, 5f);





    [Space(10)]
    [Header("General")]
    [Space(5)]

    [SerializeField]
    float rechargeTime = .5f;
    
    [Tooltip("Wait for Audio Clip to finish playing before allowing use?")]
    [SerializeField]
    bool waitForAudio = false;


    bool isUsable = true;
    Dictionary<GameObject, bool> summonTracker = new Dictionary<GameObject, bool>();

    

	public override bool Interact(PlayerController player)
    {
        if (!isUsable || !base.Interact(player))
        {
            return false;
        }


        StartCoroutine(RechargeAltar());


        DropStandardRewards(player.GetCurrentStatLevel(StatType.Luck));


        if (summonChance > 0 && Random.value <= summonChance)
            StartCoroutine(SummonObjects());

        OnUseTrigger();

        return true;
	}

    public override void Drop()
    {

    }




    IEnumerator RechargeAltar()
    {
        isUsable = false;

        yield return new WaitForSeconds(rechargeTime);

        while (waitForAudio && m_Audio.isPlaying)
        {
            yield return null;
        }
        isUsable = true;
    }


    void DropStandardRewards(int _level)
    {
        StartCoroutine(DropRewards(m_StandardListDefinition, useCustomStandard, true, TEMP_BONUS));
    }
    void DropExtinctionRewards(int _level)
    {
        StartCoroutine(DropRewards(m_RewardListDefinition, useCustomExtinction, false, TEMP_BONUS));
    }

	IEnumerator DropRewards(ListDefinitionName lstName, bool useCustom, bool isStandard, float luckBonus)
    {

        int _num = (int)rewardCountCurve.Evaluate(Random.value);


        for (int i = 0; i < _num; i++)
        {
            if (GameManager.Instance == null && !useCustom)
                continue;

            GameObject newDrop = null;
            
            if (useCustom)
            {
                newDrop = isStandard ? standardCustomItemPool.GetItem(luckBonus) : extinctionCustomItemPool.GetItem(luckBonus);
            }
            else
            {
                newDrop = GameManager.Instance.GetItem(lstName, luckBonus);
            }

          
            
            if (newDrop == null)
                continue;
            

            Vector3 newPos = transform.TransformPoint(standardSpawn.LocalOffset);
            newDrop.transform.position = newPos;
            newDrop.transform.rotation = Quaternion.identity; //= (GameObject)Instantiate(newDrop, newPos, Quaternion.identity);
            newDrop.SetActive(true);


            Rigidbody _rigidbody = newDrop.GetComponent<Rigidbody>();

            if (_rigidbody != null)
            {
                Vector3 forceVector = Random.onUnitSphere * standardSpawn.LaunchPower;
                forceVector.y = Mathf.Abs(forceVector.y);

                _rigidbody.velocity = Vector3.zero;
                _rigidbody.AddForce(forceVector);
            }

            yield return new WaitForSeconds(SPAWN_DELAY);

        }
	}
	IEnumerator SummonObjects()
    {

        summonTracker.Clear();

        int _numSummons = (int)summonCountCurve.Evaluate(Random.value);


        for (int i = 0; i < _numSummons; i++)
        {
            yield return null;


            GameObject objectToDrop = summonPool.GetItem(0f);


            if (objectToDrop == null)
                continue;



            Vector3 areaOffset = Vector3.zero;
            areaOffset.x = Random.Range(-summonSpawn.SpawnArea.x, summonSpawn.SpawnArea.x);
            areaOffset.y = Random.Range(-summonSpawn.SpawnArea.y, summonSpawn.SpawnArea.y);
            areaOffset.z = Random.Range(-summonSpawn.SpawnArea.z, summonSpawn.SpawnArea.z);


            Vector3 dropPos = transform.TransformPoint(summonSpawn.LocalOffset) + areaOffset;


            Quaternion placementQuaternion = Quaternion.AngleAxis(Random.Range(-180f, 180f), Vector3.up);
            objectToDrop = (GameObject)Instantiate(objectToDrop, dropPos, placementQuaternion);


            Vector3 bounds = Utilities.CalculateObjectBounds(objectToDrop, false);
            float checkRadius = Mathf.Max(bounds.x, Mathf.Max(bounds.y, bounds.z));

            Vector3 myBounds = Utilities.CalculateObjectBounds(gameObject, true);
            float myRadius = Mathf.Max(myBounds.x, Mathf.Max(myBounds.y, myBounds.z));


            LayerMask checkLayers = ~LayerMask.NameToLayer("Ground");
            bool validPoint;

            do
            {
                yield return null;

                dropPos = transform.position + (Random.insideUnitSphere * myRadius);
                dropPos.y = transform.position.y;

                Collider[] hitColls = Physics.OverlapSphere(dropPos, checkRadius, checkLayers);

                validPoint = true;

                for (int k = 0; k < hitColls.Length; k++)
                {
                    if (!hitColls[k].isTrigger)
                    {
                        validPoint = false;
                        break;
                    }
                }
            }
            while (!validPoint);


            Health summonHealth = objectToDrop.GetComponent<Health>();
            if (summonHealth != null)
            {
                summonTracker.Add(objectToDrop, true);
                summonHealth.OnKilled += SummonKilled;
            }


            objectToDrop.transform.position = dropPos;
            objectToDrop.SetActive(true);

        }

	}


    void SummonKilled(Health _health)
    {
        if (!summonTracker.ContainsKey(_health.gameObject))
            return;
        
        summonTracker[_health.gameObject] = false;



        if (!summonTracker.ContainsValue(true))
        {
            DropExtinctionRewards(0);

            /*
            GameObject reward = null;


            if (extinctionRewardPool == null)
            {
                return;
            }
            else
            {
                reward = extinctionRewardPool.GetLootObject();
            }



            if (reward == null)
                return;

            reward.transform.position = transform.position + (Vector3.up * REWARD_START_HEIGHT_OFFSET);
            reward.SetActive(true);
            */
        }
    }



    public override bool IsUsable
    {
        get { return base.IsUsable && isUsable && activatingObjects.Count > 0; ; }
    }
    public override bool IsUsableOutsideFOV
    { 
		get{ return true;}
	}



    void OnValidate()
    {
        Utilities.ValidateCurve_Times(rewardCountCurve, 0f, 1f);
        Utilities.ValidateCurve_Times(summonCountCurve, 0f, 1f);
    }
}
