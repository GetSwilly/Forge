using UnityEngine;
using System.Collections;
using System;

public class ExperiencePickup : MonoBehaviour, IExperienceProvider, IIdentifier, ICollectible{

    [SerializeField]
    int experienceValue;

    [SerializeField]
    string m_PickupName;

    public event EventHandler ObjectAcquired;



    public bool IsUsable
    {
        get { return true; }
    }
    public float ExperienceValue
    {
        get { return experienceValue; }
    }
    public ProviderActivationType ActivationType
    {
        get { return ProviderActivationType.Touch; }
    }

    public GameObject Object
    {
        get
        {
            return gameObject;
        }
    }
    public string Name
    {
        get { return m_PickupName; }
        set { m_PickupName = value; }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.isTrigger)
            return;

        AttemptAddExperience(coll.gameObject);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        AttemptAddExperience(other.gameObject);
    }

    void AttemptAddExperience(GameObject obj)
    {
        PlayerController _player = obj.GetComponent<PlayerController>();

        if (_player != null && _player.CanModifyExp(experienceValue))
        {

            _player.ModifyExp(experienceValue);


            if (ObjectPoolerManager.Instance != null)
            {
                GameObject _info = ObjectPoolerManager.Instance.DynamicInfoPooler.GetPooledObject();
                _info.transform.position = transform.position;

                DynamicInfoScript _infoScript = _info.GetComponent<DynamicInfoScript>();

                _info.SetActive(true);
                _infoScript.Initialize(experienceValue, Color.green);
            }


            if (ObjectAcquired != null)
            {
                ObjectAcquired(this, EventArgs.Empty);
            }

            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    public void Drop()
    {
        throw new NotImplementedException();
    }
}
