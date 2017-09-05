using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeHealer : Rune {

    [System.Serializable]
    struct HealingStruct
    {
        [SerializeField]
        [EnumFlags]
        Attribute m_Attribute;

        [SerializeField]
        AnimationCurve m_Curve;

        public Attribute Attribute
        {
            get { return m_Attribute; }
        }
        public AnimationCurve Curve
        {
            get { return m_Curve; }
        }

        public void Validate()
        {
            Utilities.ValidateCurve_Times(m_Curve, 0f, 100f);
        }
    }

    [Tooltip("Amount of healing to apply every second for each attribute")]
    [SerializeField]
    List<HealingStruct> healingAttributes = new List<HealingStruct>();

    Health ownerHealth;
    AttributeHandler ownerHandler;

    void Start()
    {
        ValidateAttributes(false);
    }



    void Update()
    {
        if (ownerHandler != null && ownerHealth != null)
        {
            for (int i = 0; i < healingAttributes.Count; i++)
            {
                float healthDelta = ownerHandler.GetActiveAttributeAmount(healingAttributes[i].Attribute) * Time.deltaTime;

                ownerHealth.HealthArithmetic(healthDelta, false, m_Transform);
            }
        }
    }



    public override void Initialize(UnitController _unit)
    {
        base.Initialize(_unit);

        if (Owner != null)
        {
            ownerHealth = Owner.GetComponent<Health>();
            ownerHandler = Owner.GetComponent<AttributeHandler>();
        }
    }
    public override void Terminate()
    {
        base.Terminate();
        ownerHealth = null;
    }




    void ValidateAttributes(bool allowFinalDuplicate)
    {
        HashSet<Attribute> encounteredSet = new HashSet<Attribute>();

        for (int i = 0; i < healingAttributes.Count; i++)
        {
            if (allowFinalDuplicate && i == healingAttributes.Count - 1)
            {
                continue;
            }


            if (encounteredSet.Contains(healingAttributes[i].Attribute))
            {
                healingAttributes.RemoveAt(i);
                i--;
            }
            else
            {
                encounteredSet.Add(healingAttributes[i].Attribute);
            }
        }


    }

    void OnValidate()
    {
        ValidateAttributes(true);
    }
}
