using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsPickup : MonoBehaviour, ICollectible
{

    [SerializeField]
    int creditValue;

    public event EventHandler ObjectAcquired;



    public bool IsUsable
    {
        get { return true; }
    }
    public int Value
    {
        get { return creditValue; }
        set { creditValue = Mathf.Clamp(value, 0, value); }
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
        UnitController _controller = coll.gameObject.GetComponent<UnitController>();

        if (_controller != null && _controller.Charge(Value))
        {
            ObjectAcquired(this, EventArgs.Empty);
            
            Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        Value = Value;
    }
}
