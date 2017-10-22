using UnityEngine;
using System.Collections;
using System;

public class HealthPickup : MonoBehaviour,IHealthProvider, ICollectible
{
    

    [SerializeField]
    float healthValue;

    public event EventHandler ObjectAcquired;



    public bool IsUsable
    {
        get{ return true; }
    }
    public float HealthValue
    {
        get { return healthValue; }
        set { healthValue = Mathf.Clamp(value, 0f, value); }
    }
    public ProviderActivationType ActivationType
    {
        get { return ProviderActivationType.Touch; }
    }
    
    void OnCollisionEnter(Collision coll)
    {
        Health _health = coll.gameObject.GetComponent<Health>();

        if (_health != null && _health.NeedsHealth)
        {
            _health.HealthArithmetic(healthValue, false, transform);

            if (ObjectPoolerManager.Instance != null)
            {
                GameObject _info = ObjectPoolerManager.Instance.DynamicInfoPooler.GetPooledObject();
                _info.transform.position = transform.position;

                DynamicInfoScript _infoScript = _info.GetComponent<DynamicInfoScript>();

                _info.SetActive(true);
                _infoScript.Initialize((int)healthValue, Color.red);
            }


            if (ObjectAcquired != null)
            {
                ObjectAcquired(this, EventArgs.Empty);
            }

            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        HealthValue = HealthValue;
    }
}
