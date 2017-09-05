using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AttributeEffect {
    
    public delegate void AttributeChanged(AttributeEffect _attribute);
    public AttributeChanged OnAttributeChange;


    private Attribute m_Attribute;

    private float burstRate = 0f;
    private float healthDelta = 0f;

    private float decayRate = 0f;
    private float transmissionRate = 0f;
    private float currentValue = 0f;

    private float spreadThreshold = 0f;


    private bool shouldSpread = false;
    


    float burstTimer = 0f;

    protected Transform m_Owner;
    protected Transform m_ActivatorTransform;
    protected AttributeHandler m_Handler;
    protected Health m_Health;




    public AttributeEffect(Transform _owner, AttributeHandler _handler, Transform _activator)
    {
        m_Owner = _owner;
        m_Handler = _handler;
        m_ActivatorTransform = _activator;

        m_Health = m_Owner.GetComponent<Health>();
        

        currentValue = 1f;
    }




    public virtual void Update()
    {
        ModifyValue(DecayRate * Time.deltaTime);
        DamageBurstCheck();
    }
    void DamageBurstCheck()
    {
        burstTimer += Time.deltaTime;

        if(burstTimer >= BurstRate)
        {
            PerformBurstEffect();
            burstTimer = 0f;
        }
    }
    protected void PerformBurstEffect()
    {
        if (!HealthDelta.Equals(0f))
        {
            m_Health.HealthArithmetic(HealthDelta, false, m_ActivatorTransform);
        }
    }




    public virtual void Deactivate()
    {
        m_Handler.RemoveActiveAttribute(Attribute);
       // m_Handler.StopCoroutines(MyCoroutines);
     
    }

    int counter = 0;

   


  

    public void ModifyValue(float delta)
    {
        CurrentValue += delta;
        //Debug.Log(string.Format("Current Value: {0}. Delta: {1}",CurrentValue,delta));

        if (CurrentValue <= 0)
        {
            Deactivate();
        }

        if (OnAttributeChange != null)
        {
            OnAttributeChange(this);
        }
    }
    public void SetEnergy(float newEnergy)
    {
        ModifyValue(newEnergy - currentValue);
    }



    public float GetPercentage()
    {
        return Mathf.Clamp01(currentValue / 100);
    }


    #region Accessors 
    
    public float CurrentValue
    {
        get { return currentValue; }
        private set
        {
            currentValue = Mathf.Clamp(value, 0, 100);
        }
    }

    public float BurstRate
    {
        get { return burstRate; }
        protected set { burstRate = value; }
    }
    public float HealthDelta
    {
        get { return healthDelta; }
        protected set { healthDelta = value; }
    }

    public float DecayRate
    {
        get { return decayRate; }
        protected set { decayRate = value; }
    }
    public float TransmissionRate
    {
        get { return transmissionRate; }
        protected set { transmissionRate = value; }
    }


    public bool ShouldSpread
    {
        get { return shouldSpread; }
    }
    public float SpreadThreshold
    {
        get { return spreadThreshold; }
        protected set { spreadThreshold = value; }
    }
    public Attribute Attribute
    {
        get { return m_Attribute; }
        protected set { m_Attribute = value; }
    }


    #endregion



    protected virtual bool ShouldSpreadOnCollision(Collision coll)
    {
        if (!ShouldSpread)
            return false;

        if (coll.gameObject.tag == "Ground")
            return false;
        
        if (GetPercentage() < SpreadThreshold)
            return false;


        return true;
    }


    //protected void AddAttributeToGameObject(GameObject)
    //{
    //    float resistanceValue = m_Handler == null ? 1f : m_Handler.GetResistanceMultiplier(Attribute);

    //    FireAttribute _effect = coll.gameObject.GetComponent<FireAttribute>();

    //    if (_effect == null)
    //    {
    //        _effect = coll.gameObject.AddComponent<FireAttribute>();
    //        _effect.Activate(transform);
    //    }

    //    _effect.ModifyValue(TransmissionRate * resistanceValue * Time.deltaTime);
    //}

    public virtual void HandleCollision(Collision coll)
    {
        if (ShouldSpreadOnCollision(coll))
        {
            AttributeHandler _handler = coll.gameObject.GetComponent<AttributeHandler>();

            if(_handler == null)
            {
                _handler = coll.gameObject.AddComponent<AttributeHandler>();
            }

            _handler.ModifyActiveAttribute(Attribute, TransmissionRate * Time.deltaTime, m_Owner);
        }
    }
}
