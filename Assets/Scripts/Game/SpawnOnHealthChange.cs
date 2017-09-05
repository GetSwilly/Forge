using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
public class SpawnOnHealthChange : MonoBehaviour {

    static readonly float SPAWNING_DELAY = 0.2f;


    public enum SpawnActivationType { OnDamage, OnHealthPercentage };
    public SpawnActivationType myActivationType = SpawnActivationType.OnHealthPercentage;

    public enum PercentageSide { Above, Below };
    public PercentageSide myPercentageSide = PercentageSide.Below;

    [SerializeField]
    [Range(0f, 1f)]
    float healthPercentage = 0.5f;

    [SerializeField]
    int numberOfActivations = 1;

    [SerializeField]
    float rechargeDelay = 1f;
    bool isRecharging = false;

    [SerializeField]
    List<SpawnObject> objectsToSpawn = new List<SpawnObject>();


    Health myHealth;

    void Awake()
    {
        myHealth = GetComponent<Health>();
        myHealth.OnHealthChange += CheckHealth;
    }


    void CheckHealth(Health _health)
    {
        if (isRecharging)
            return;


        if(myActivationType == SpawnActivationType.OnDamage && myHealth.LastHealthChange < 0)
        {
            StartCoroutine(Spawn());
        }
        else if(myActivationType == SpawnActivationType.OnHealthPercentage && myHealth.HealthPercentage <= healthPercentage && myPercentageSide == PercentageSide.Below)
        {
            StartCoroutine(Spawn());
        }
        else if (myActivationType == SpawnActivationType.OnHealthPercentage && myHealth.HealthPercentage >= healthPercentage && myPercentageSide == PercentageSide.Above)
        {
            StartCoroutine(Spawn());
        }
    }

    IEnumerator Spawn()
    {
        
        for(int i = 0; i < objectsToSpawn.Count; i++)
        {
            if (objectsToSpawn[i].objectPrefab == null)
                continue;

            Vector3 startPos = transform.TransformPoint(objectsToSpawn[i].spawnPosition);
            for (int k = 0; k < objectsToSpawn[i].numToSpawn; k++)
            {
                GameObject obj = (GameObject)Instantiate(objectsToSpawn[i].objectPrefab,startPos + (Random.insideUnitSphere * objectsToSpawn[i].spawnRadius), transform.rotation);
                obj.SetActive(true);


                Rigidbody _rigid = obj.GetComponent<Rigidbody>();
                if(_rigid != null)
                {
                    _rigid.velocity = Vector3.zero;

                    Vector3 spawnDirection = Random.insideUnitSphere;
                    spawnDirection.y *= spawnDirection.y < 0 ? -1 : 1;

                    _rigid.AddForce(spawnDirection.normalized, ForceMode.Impulse);
                }


                yield return new WaitForSeconds(SPAWNING_DELAY);
            }
        }


        numberOfActivations--;
        if (numberOfActivations <= 0)
            Destroy(this);
    }


    IEnumerator RechargeSpawn()
    {
        isRecharging = true;
        yield return new WaitForSeconds(rechargeDelay);
        isRecharging = false;
    }
}


[System.Serializable]
struct SpawnObject
{
    public GameObject objectPrefab;
    public int numToSpawn;

    public Vector3 spawnPosition;
    public float spawnRadius;
    public float spawnForce;
}