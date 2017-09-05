using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AttributeHandler : MonoBehaviour
{

    [Serializable]
    class AttributeResistance
    {
        [SerializeField]
        Attribute m_Attribute;

        [SerializeField]
        [Range(0f, 1f)]
        float m_Resistance;


        public AttributeResistance(Attribute _attribute) : this(_attribute, 0f) { }
        public AttributeResistance(Attribute _attribute, float _value)
        {
            m_Attribute = _attribute;
            Resistance = Resistance;
        }

        public Attribute Attribute
        {
            get { return m_Attribute; }
        }
        public float Resistance
        {
            get { return m_Resistance; }
            set { m_Resistance = Mathf.Clamp01(value); }
        }
    }



    [SerializeField]
    List<AttributeResistance> m_Resistances = new List<AttributeResistance>();

    [SerializeField]
    List<AttributeEffect> activeAttributes = new List<AttributeEffect>();


    [SerializeField]
    bool showDebug = false;

    List<string> activeTrackers = new List<string>();


    UnitUI m_UI;
    Transform m_Transform;


    void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }
    

    void Update()
    {
        for (int i = 0; i < activeAttributes.Count; i++)
        {
            activeAttributes[i].Update();
        }
    }



    #region Resistance

    public void AddAttributeResistance(Attribute _attribute, float _amount)
    {
        m_Resistances.Add(new AttributeResistance(_attribute, _amount));
        ValidateResistances(false);
    }
    public float GetResistanceMultiplier(Attribute _attribute)
    {
        int index = GetAttributeResistanceIndex(_attribute);

        if (index == -1)
            return 1;

        return 1f - m_Resistances[index].Resistance;
    }




    public void RemoveAttributeResistance(Attribute _attribute)
    {
        int index = GetAttributeResistanceIndex(_attribute);

        if (index == -1)
            return;

        m_Resistances.RemoveAt(index);
    }
    public void ModifyAttributeResistance(Attribute _attribute, float _delta)
    {
        int index = GetAttributeResistanceIndex(_attribute);

        if (index != -1)
        {
            m_Resistances[index].Resistance += _delta;
        }
        else
        {
            AddAttributeResistance(_attribute, _delta);
        }
    }
    public void MultiplyAttributeResistance(Attribute _attribute, float _multiplier)
    {
        int index = GetAttributeResistanceIndex(_attribute);

        if (index == -1)
            return;

        m_Resistances[index].Resistance *= _multiplier;
    }
    public void SetAttributeResistance(Attribute _attribute, float _amount)
    {
        int index = GetAttributeResistanceIndex(_attribute);

        if (index != -1)
        {
            m_Resistances[index].Resistance = _amount;
        }
        else
        {
            AddAttributeResistance(_attribute, _amount);
        }
    }

    int GetAttributeResistanceIndex(Attribute _attribute)
    {
        for (int i = 0; i < m_Resistances.Count; i++)
        {
            if (m_Resistances[i].Attribute == _attribute)
                return i;
        }

        return -1;
    }

    #endregion


    #region Active

    void AddActiveAttribute(Attribute _attribute, float _amount, Transform _activator)
    {
        AttributeEffect attr = null;
        switch (_attribute)
        {
            case Attribute.Fire:
                attr = new FireAttribute(transform, this, _activator);
                break;
            case Attribute.Ice:
                attr = new IceAttribute(transform, this, _activator);
                break;
            case Attribute.Poison:
                attr = new PoisonAttribute(transform, this, _activator);
                break;
            case Attribute.Shock:
                attr = new ShockAttribute(transform, this, _activator);
                break;
            case Attribute.Water:
                attr = new WaterAttribute(transform, this, _activator);
                break;
            case Attribute.Visibility:
                attr = new VisibilityAttribute(transform, this, _activator);
                break;
            default:
                throw new NotImplementedException();
        }

        attr.OnAttributeChange += AttributeChanged;
        activeAttributes.Add(attr);

        if (showDebug)
        {
            Debug.Log("Adding Attribute: " + attr);
        }

        SetActiveAttribute(_attribute, _amount, _activator);
    }

    public void RemoveActiveAttribute(Attribute _attribute)
    {
        int index = GetActiveAttributeIndex(_attribute);

        if (index == -1)
            return;

        RemoveUI(activeAttributes[index]);
        activeAttributes[index].OnAttributeChange -= AttributeChanged;

        activeAttributes.RemoveAt(index);
    }

    public void RemoveAllActiveAttributes()
    {
        while(activeAttributes.Count > 0)
        {
            RemoveActiveAttribute(activeAttributes[0].Attribute);
        }
    }

    public void ModifyActiveAttribute(Attribute _attribute, float _delta, Transform _activator)
    {
        if (showDebug)
        {
            Debug.Log(string.Format("Modifying attribute: {0}. Delta: {1}", _attribute.ToString(), _delta.ToString()));
        }



        int index = GetActiveAttributeIndex(_attribute);
        _delta *= GetResistanceMultiplier(_attribute);

        if (index != -1)
        {
            activeAttributes[index].ModifyValue(_delta);
        }
        else
        {
            AddActiveAttribute(_attribute, _delta, _activator);
        }
    }

    public void SetActiveAttribute(Attribute _attribute, float _amount, Transform _activator)
    {
        int index = GetActiveAttributeIndex(_attribute);

        if (index != -1)
        {
            activeAttributes[index].SetEnergy(_amount);
        }
        else
        {
            AddActiveAttribute(_attribute, _amount, _activator);
        }
    }

    int GetActiveAttributeIndex(Attribute _attribute)
    {
        for (int i = 0; i < activeAttributes.Count; i++)
        {
            if (activeAttributes[i].Attribute == _attribute)
                return i;
        }

        return -1;
    }
    
    public bool HasActiveAttribute(Attribute _attribute)
    {
        return GetActiveAttributeIndex(_attribute) != -1;
    }
    
    public float GetActiveAttributeAmount(Attribute _attribute)
    {
        if (HasActiveAttribute(_attribute))
            return 0f;

        int index = GetActiveAttributeIndex(_attribute);

        return activeAttributes[index].CurrentValue;
    }

    #endregion



    public void AddTracker(Transform target)
    {
        if (target == null || target.gameObject.activeInHierarchy || activeTrackers.Contains(NameTargetTracker(target)))
            return;

        GameObject _trackerPrefab = UI.GetPrefab("Tracker");

        if (_trackerPrefab == null)
            return;


        TrackGoal _trackerScript = _trackerPrefab.GetComponent<TrackGoal>();
        _trackerScript.Target = target;

        UI.AddAttribute(new GenericUI.DisplayProperties(NameTargetTracker(target), new Orientation(new Vector3(0,0,1), Vector3.zero, Vector3.one), _trackerScript));
        activeTrackers.Add(NameTargetTracker(target));
    }
    public void RemoveTracker(Transform target)
    {
        UI.RemoveAttribute(NameTargetTracker(target));
        activeTrackers.Remove(NameTargetTracker(target));
    }



    public void AttributeChanged(AttributeEffect _effect)
    {
        UpdateUI(_effect);

        if (_effect.GetPercentage().Equals(0f))
        {
            RemoveActiveAttribute(_effect.Attribute);
        }
    }


    //public void StartCoroutines(List<IEnumerator> _coroutines)
    //{
    //    for (int i = 0; i < _coroutines.Count; i++)
    //    {
    //        StartCoroutine(_coroutines[i]);
    //    }
    //}
    //public void StopCoroutines(List<IEnumerator> _coroutines)
    //{
    //    for (int i = 0; i < _coroutines.Count; i++)
    //    {
    //        StopCoroutine(_coroutines[i]);
    //    }
    //}

    void GetUI()
    {
        if (ObjectPoolerManager.Instance == null)
            return;

        if (m_UI != null || ObjectPoolerManager.Instance.Unit_UIPooler == null)
            return;



        GameObject g = ObjectPoolerManager.Instance.Unit_UIPooler.GetPooledObject();
        m_UI = g.GetComponent<UnitUI>();

        if (m_UI == null)
        {
            m_UI = null;
            g.SetActive(false);
            return;
        }


        //  g.transform.position = myTransform.position;
        //  g.SetActive(true);
        // myUI.Initialize(myTransform, false);
    }
    public void ShowUI(bool shouldShowExp)
    {
        if (UI == null || m_Transform == null)
            return;


        if (!UI.gameObject.activeInHierarchy)
        {
            UI.gameObject.SetActive(true);
            UI.transform.position = m_Transform.position;
        }

        UI.SetFollowOffset(Vector3.zero);

        UI.Initialize(m_Transform, false, shouldShowExp);

        UpdateUI();
    }

    public void HideUI()
    {
        //Debug.Log($"{Time.realtimeSinceStartup} --{myTransform.name} --- Hide UI");

        if (UI == null)
            return;
        
        UI.Terminate();
    }
    public void UpdateUI()
    {
        //throw new NotImplementedException();
    }
    public void UpdateUI(Attribute attr, float percentage, bool shouldSetImmediate)
    {
        UI.UpdateAttribute(attr.ToString(), percentage);
    }
    public void UpdateUI(string attrString, float percentage, bool shouldSetImmediate)
    {
        //Debug.Log(string.Format("Updating attribute: {0} to percentage {1} %", attrString , percentage* 100f));

        if (!UI.HasAttribute(attrString))
        {
            GameObject attrObject = UI.GetPrefab("Attribute");
            DisplayUI attrUI = attrObject.GetComponent<DisplayUI>();

            if (attrUI != null)
            {
                Debug.Log(Utilities.GetAttributeColor(attrString));

                attrUI.SetColor(Utilities.GetAttributeColor(attrString));
            }

            //Debug.Log("Adding attribute: " + attrString);
            UI.AddAttribute(new GenericUI.DisplayProperties(attrString, new Orientation(Vector3.zero, Vector3.zero, Vector3.one), attrUI), "Attributes");
        }

        UI.UpdateAttribute(attrString, percentage);

    }
    public void UpdateUI(AttributeEffect _effect)
    {
        GetUI();

        if (!UI.HasAttribute(_effect.Attribute.ToString()))
        {
            GameObject attrObject = UI.GetPrefab("Attribute");
            attrObject.name = _effect.Attribute.ToString();
            DisplayUI attrUI = attrObject.GetComponent<DisplayUI>();

            if (attrUI != null)
            {
                attrUI.SetColor(Utilities.GetAttributeColor(_effect.Attribute));
            }

            // Debug.Log("Adding attribute: " + _effect.Attribute.ToString());
            UI.AddAttribute(new GenericUI.DisplayProperties(_effect.Attribute.ToString(), new Orientation(Vector3.zero, Vector3.zero, Vector3.one), attrUI), "Attributes");
        }

        UI.UpdateAttribute(_effect.Attribute.ToString(), _effect.GetPercentage());
    }


    public void RemoveUI(AttributeEffect _effect)
    {
        if (UI.HasAttribute(_effect.Attribute.ToString()))
        {
            UI.RemoveAttribute(_effect.Attribute.ToString());

            if (showDebug)
            {
                Debug.Log(m_Transform.name + " -- AttributeHandler --- RemoveUI() -- Removing Attribute: " + _effect.Attribute);
            }
        }
        else if (showDebug)
        {
            Debug.Log(m_Transform.name + " -- AttributeHandler --- RemoveUI() -- Does not contain Attribute: " + _effect.Attribute + ". Can't remove.");
        }
    }





    void OnCollision(Collision coll)
    {
        for (int i = 0; i < activeAttributes.Count; i++)
        {
            activeAttributes[i].HandleCollision(coll);
        }
    }



    void ValidateResistances(bool allowFinalDuplicate)
    {
        HashSet<Attribute> encounteredSet = new HashSet<Attribute>();

        for (int i = 0; i < m_Resistances.Count; i++)
        {
            if (allowFinalDuplicate && i == m_Resistances.Count - 1)
            {
                continue;
            }


            if (encounteredSet.Contains(m_Resistances[i].Attribute))
            {
                m_Resistances.RemoveAt(i);
                i--;
            }
            else
            {
                encounteredSet.Add(m_Resistances[i].Attribute);
            }
        }


    }

    //void ValidateEffects()
    //{
    //    HashSet<Attribute> encounteredSet = new HashSet<Attribute>();

    //    for (int i = 0; i < m_Resistances.Count; i++)
    //    {
    //        if (encounteredSet.Contains(m_Resistances[i].Attribute))
    //        {
    //            m_Resistances.RemoveAt(i);
    //            i--;
    //        }
    //        else
    //        {
    //            encounteredSet.Add(m_Resistances[i].Attribute);
    //        }
    //    }
    //}

    protected UnitUI UI
    {
        get
        {
            if (m_UI == null)
            {
                GetUI();
            }


            return m_UI;
        }
    }



    void OnValidate()
    {
        ValidateResistances(true);
        //ValidateEffects();
    }


    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("Active Attributes: ");
        for(int i = 0; i < activeAttributes.Count; i++)
        {
            sb.Append(m_Resistances[i].Attribute);
            sb.Append(",");
            sb.Append(m_Resistances[i].Resistance);
            sb.Append(".");
        }
        


        sb.Append(" Resistances: ");
        for (int i = 0; i < m_Resistances.Count; i++)
        {
            sb.Append(m_Resistances[i].Attribute);
            sb.Append(",");
            sb.Append(m_Resistances[i].Resistance);
            sb.Append(".");
        }


        return sb.ToString();
    }


    public static string NameTargetTracker(Transform target)
    {
        return target.ToString() + " (" + target.GetInstanceID() + ") Tracker";
    }
    //void AddAttributeEffect(IAttributeEffect _newEffect)
    //{
    //    for (int i = 0; i < activeEffects.Count; i++)
    //    {
    //        if (activeEffects[i] == _newEffect)
    //            return;
    //    }

    //    activeEffects.Add(_newEffect);
    //    //_newEffect.SubscribeToOnPercentageChange(UpdateElementUI);
    //}
    //public void UpdateAttributeUI(IAttributeEffect _effect)
    //{
    //    if (myUI == null)
    //        return;


    //    myUI.UpdateAttributeUI(_effect, _percentage);
    //}
}