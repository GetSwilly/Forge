using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShockAttribute : AttributeEffect {
    
    static readonly float spreadChance = 0.2f;
   
    float shockRateMean = 4f;
    float shockRateSigma = 1.5f;
    
    float shockEffectDelta = 50f;

    float shockRadius = 5f;

    
    [SerializeField]
    LayerMask friendlyMask;

    
    Dictionary<GameObject, bool> shockDictionary = new Dictionary<GameObject, bool>();

    float shockNearbyTimer = 0f;



    public ShockAttribute(Transform _owner, AttributeHandler _handler, Transform _activator) : base(_owner, _handler, _activator)
    {
        Attribute = Attribute.Shock;
        DecayRate = -10f;
        TransmissionRate = 30f;

        BurstRate = 2f;
        HealthDelta = 10f;

        SpreadThreshold = 0.5f;
    }
    
   
    public override void Update()
    {
        base.Update();
        ShockNearbyCheck();
    }
    void ShockNearbyCheck()
    {
        shockNearbyTimer -= Time.deltaTime;

        if (shockNearbyTimer <= 0f)
        {
            ShockNearby();
            shockNearbyTimer = (float)Utilities.GetRandomGaussian(shockRateMean, shockRateSigma);
        }
    }
    void ShockNearby()
    {
            shockDictionary.Clear();

            shockDictionary.Add(m_Owner.gameObject, true);


            if (m_Health != null)
                m_Health.HealthArithmetic(-HealthDelta, false, m_ActivatorTransform);


            ShockNearby(m_Owner.position, 1);
    }
    void ShockNearby(Vector3 originPosition, int iterationNum)
    {

        List<GameObject> spreadObjects = new List<GameObject>();
        Collider[] nearbyColliders = Physics.OverlapSphere(originPosition, GetShockRadius(iterationNum));

        for (int i = 0; i < nearbyColliders.Length; i++)
        {

            //Ignore if this gameobject, in friendly mask, is ground, or has already been considered
            if (nearbyColliders[i].gameObject == m_Owner.gameObject || Utilities.IsInLayerMask(nearbyColliders[i].gameObject, friendlyMask) || nearbyColliders[i].gameObject.tag == "Ground" || shockDictionary.ContainsKey(nearbyColliders[i].gameObject))
                continue;
            

            AttributeHandler _handler = nearbyColliders[i].GetComponent<AttributeHandler>();

            if (_handler == null)
                _handler = nearbyColliders[i].gameObject.AddComponent<AttributeHandler>();


            float resistanceMultiplier = _handler == null ? 1 : _handler.GetResistanceMultiplier(Attribute);

            //Ignore if complete resistance
            if (resistanceMultiplier.Equals(0f))
                continue;


            //Chance to add Lightning Effect
            if (UnityEngine.Random.value <= GetSpreadChance(iterationNum))
            {
                if (_handler == null)
                    _handler = nearbyColliders[i].gameObject.AddComponent<AttributeHandler>();

                _handler.ModifyActiveAttribute(Attribute, shockEffectDelta, m_Owner);
            }


            //Apply shock damage
            Health _health = nearbyColliders[i].GetComponent<Health>();

            if (_health != null)
                _health.HealthArithmetic(-HealthDelta * resistanceMultiplier, false, m_ActivatorTransform);



            //Chance to arc lightning through object
            if (UnityEngine.Random.value <= GetSpreadChance(iterationNum))
                spreadObjects.Add(nearbyColliders[i].gameObject);


            shockDictionary.Add(nearbyColliders[i].gameObject, true);
        }

        //Recursively shock other objects
        for (int i = 0; i < spreadObjects.Count; i++)
        {
            ShockNearby(spreadObjects[i].transform.position, iterationNum + 1);
        }
    }




    


    float GetShockRadius(int iteration)
    {
        return shockRadius;
    }
    float GetSpreadChance(int iteration)
    {
        return spreadChance;
    }



    protected override bool ShouldSpreadOnCollision(Collision coll)
    {
        if (!base.ShouldSpreadOnCollision(coll))
            return false;

        if (UnityEngine.Random.value > spreadChance)
            return false;

        return true;
    }



    //public override List<IEnumerator> MyCoroutines
    //{
    //    get
    //    {
    //        List<IEnumerator> _coroutines = base.MyCoroutines;
    //        _coroutines.Add(ShockNearbyRoutine());

    //        return _coroutines;
    //    }
    //}
}
