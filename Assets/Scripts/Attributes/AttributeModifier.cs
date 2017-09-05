using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeModifier : MonoBehaviour {
    
    [SerializeField]
    List<Tuple_AttributeFloat> attributeModifications = new List<Tuple_AttributeFloat>();

    List<GameObject> affectedObjects = new List<GameObject>();


    [SerializeField]
    bool applyOnContact = true;


    Transform m_Transform;



    void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }

    
    void OnDisable()
    {
        StopAllCoroutines();
    }




    void Update()
    {
        for (int k = 0; k < affectedObjects.Count; k++)
        {
            AttributeHandler _handler = affectedObjects[k].GetComponent<AttributeHandler>();

            if (_handler == null)
                _handler = affectedObjects[k].AddComponent<AttributeHandler>();


            for (int i = 0; i < attributeModifications.Count; i++)
            {
                if (attributeModifications[i].Item2 == 0)
                    continue;

                float transmissionVal = attributeModifications[i].Item2 * Time.deltaTime;

                _handler.ModifyActiveAttribute(attributeModifications[i].Item1, transmissionVal, m_Transform);
            }
        }
    }




    public void AddEffect(Attribute _type, float _amt)
    {
        attributeModifications.Add(new Tuple_AttributeFloat(_type, _amt));
        ValidateEffects(false);
    }
    public void RemoveEffect(Attribute _type)
    {
        int index = GetEffectIndex(_type);

        if (index == -1)
            return;

        attributeModifications.RemoveAt(index);
    }
    public void AlterEffect(Attribute _type, float delta)
    {
        int index = GetEffectIndex(_type);

        if (index == -1)
            return;

        attributeModifications[index].Item2 += delta;
    }
    public void MultiplyEffect(Attribute _type, float _mult)
    {
        int index = GetEffectIndex(_type);

        if (index == -1)
            return;

        attributeModifications[index].Item2 *= _mult;
    }
    public void SetEffect(Attribute _type, float amt)
    {
        int index = GetEffectIndex(_type);

        if (index == -1)
            return;

        attributeModifications[index].Item2 = amt;
    }


    int GetEffectIndex(Attribute _type)
    {
        for (int i = 0; i < attributeModifications.Count; i++)
        {
            if (attributeModifications[i].Item1 == _type)
                return i;
        }

        return -1;
    }


    //IEnumerator ApplyEffects()
    //{
    //    while (true)
    //    {
    //        yield return null;


    //        for (int k = 0; k < affectedObjects.Count; k++)
    //        {
    //            AttributeHandler _handler = affectedObjects[k].GetComponent<AttributeHandler>();

    //            if (_handler == null)
    //                _handler = affectedObjects[k].AddComponent<AttributeHandler>();


    //            for (int i = 0; i < attributeModifications.Count; i++)
    //            {

    //                if (attributeModifications[i].Item2 == 0)
    //                    continue;

    //                float transmissionVal = attributeModifications[i].Item2 * Time.deltaTime;

    //                _handler.ModifyActiveAttribute(attributeModifications[i].Item1, transmissionVal, m_Transform);
    //            }
    //        }
    //    }
    //}



    public void AddObject(Collider coll)
    {
        if (coll.isTrigger)
            return;

        AddObject(coll.gameObject);
    }
    public void AddObject(GameObject obj)
    {
        if (obj.name == "Ground")
            return;

        if (!affectedObjects.Contains(obj))
            affectedObjects.Add(obj);

    }
    public void RemoveObject(GameObject obj)
    {
        if (affectedObjects.Contains(obj))
            affectedObjects.Remove(obj);
    }
    public void RemoveAllObjects()
    {
        affectedObjects.Clear();
    }



    public bool AutoEffect
    {
        get { return applyOnContact; }
        set { applyOnContact = value; }
    }




    void OnCollisionStay(Collision coll)
    {
        if (!applyOnContact || coll.collider.isTrigger)
            return;

        AddObject(coll.collider);

    }
    void OnCollisionExit(Collision coll)
    {
        if (coll.collider.isTrigger)
            return;
        
        RemoveObject(coll.gameObject);
    }







    void ValidateEffects(bool shouldIncludeInspectorBuffer)
    {
        HashSet<Attribute> _dict = new HashSet<Attribute>();
        List<Tuple_AttributeFloat> _effects = new List<Tuple_AttributeFloat>();


        for (int i = 0; i < attributeModifications.Count - 1; i++)
        {
            if (!_dict.Contains(attributeModifications[i].Item1))
            {
                _effects.Add(attributeModifications[i]);
                _dict.Add(attributeModifications[i].Item1);
            }
        }

        if (attributeModifications.Count > 0)
        {
            Tuple_AttributeFloat tempTuple = attributeModifications[attributeModifications.Count - 1];

            if (shouldIncludeInspectorBuffer || !_dict.Contains(tempTuple.Item1))
            {
                _effects.Add(tempTuple);
            }
        }


        attributeModifications = _effects;

        //Attribute[] _types = (Attribute[])Enum.GetValues(typeof(Attribute));
        //for (int i = 0; i < _types.Length; i++)
        //{
        //    if (_types[i] == Attribute.Neutral)
        //        continue;

        //    if (!_dict.ContainsKey(_types[i]))
        //    {
        //        _effects.Add(new Tuple_AttributeFloat(_types[i], 0f));
        //        _dict.Add(_types[i], true);
        //    }
        //}
    }


    void OnValidate()
    {
        ValidateEffects(true);
    }
}
