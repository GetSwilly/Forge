using UnityEngine;
using System.Collections;
using System;

public class HealthPickup : MonoBehaviour,IHealthProvider,IAcquirableObject {
    

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
    }
    public ProviderActivationType ActivationType
    {
        get { return ProviderActivationType.Touch; }
    }

    public GameObject Object
    {
        get
        {
            return this.gameObject;
        }
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


            ObjectAcquired(this, EventArgs.Empty);

            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    public void Drop()
    {
        throw new NotImplementedException();
    }
}
