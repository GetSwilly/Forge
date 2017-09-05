using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(AudioSource))]
public abstract class InteractableObject : MonoBehaviour, IAcquirableObject, IIdentifier
{
    [SerializeField]
    string m_ObjectName = "Interactable Object";

    [SerializeField]
    Vector3 m_UIOffset = new Vector3(0, 1,2);

    [SerializeField]
    protected bool shouldShowLines = true;

    [SerializeField]
    protected List<Cost> m_ActivationCosts = new List<Cost>();



    [Space(10)]
    [Header("Sounds")]
    [Space(5)]
    
    [SerializeField]
    protected SoundClip activationSound;



    [SerializeField]
    [EnumFlags]
    UIManager.Component m_UIComponents;




    public delegate void AlertEvent();

    public event UIManager.UIEvent OnUI_Inflate;
    public event UIManager.UIEvent OnUI_Deflate;
    public event AlertEvent OnUse;
    public event AlertEvent OnGive;
    public event EventHandler ObjectAcquired;

    protected List<GameObject> activatingObjects = new List<GameObject>();
    

    GenericUI activeUI;
    protected Transform m_Transform;
    protected AudioSource m_Audio;



    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Audio = GetComponent<AudioSource>();
        m_Audio.loop = false;
        m_Audio.playOnAwake = false;
    }



    public virtual bool Use(PlayerController player)
    {
        if (activatingObjects.Count == 0)
            return false;


        bool isActivator = false;

        for (int i = 0; i < activatingObjects.Count; i++)
        {
            if (activatingObjects[i] == player.gameObject)
                isActivator = true;
        }

        if (!isActivator)
            return false;



      

        Health playerHealth = player.GetComponent<Health>();



        //if(Utilities.HasFlag(m_ActivationCosts.Currency, CurrencyType.Experience))
        //{
        //    expSum = m_ActivationCosts.ExperienceCost;
        //}

        //if (Utilities.HasFlag(m_ActivationCosts.Currency, CurrencyType.Health))
        //{
        //    healthSum = m_ActivationCosts.ExperienceCost;
        //}

        //if (Utilities.HasFlag(m_ActivationCosts.Currency, CurrencyType.LevelPoints))
        //{
        //    throw new NotImplementedException();
        //}

        //if (Utilities.HasFlag(m_ActivationCosts.Currency, CurrencyType.StatLevel))
        //{
        //    throw new NotImplementedException();
        //}

        int expSum = 0;
        int healthSum = 0;
        int levelSum = 0;
        Dictionary<StatType, int> statSum = new Dictionary<StatType, int>();

        for (int i = 0; i < ActivationCosts.Count; i++)
        {
            switch (ActivationCosts[i].Currency)
            {
                case CurrencyType.Experience:
                    expSum += ActivationCosts[i].Value;
                    break;
                case CurrencyType.Health:
                    healthSum += ActivationCosts[i].Value;
                    break;
                case CurrencyType.LevelPoints:
                    levelSum += ActivationCosts[i].Value;
                    break;
                case CurrencyType.StatLevel:
                    if (statSum.ContainsKey(ActivationCosts[i].StatType))
                    {
                        statSum[ActivationCosts[i].StatType] += ActivationCosts[i].Value;
                    }
                    else
                    {
                        statSum.Add(ActivationCosts[i].StatType, ActivationCosts[i].Value);
                    }
                    break;
            }

        }



        bool isSuccess = player.CanModifyExp(Mathf.RoundToInt(expSum));

        if (!isSuccess)
            return false;



        if (expSum != 0)
            player.ModifyExp(Mathf.RoundToInt(expSum));


        if (healthSum != 0)
            playerHealth.HealthArithmetic(healthSum, false, transform);


        OnUseTrigger();

        PlaySound(activationSound);

        return true;
    }
    public abstract bool Give(PlayerController player);



    public abstract void Drop();

    /*
    protected bool CanAfford(CurrencyType _currency)
    {
        if (activationCosts.Count == 0)
            return true;


        float totalCost = 0;
        for(int i = 0; i < activationCosts.Count; i++)
        {
            if(activationCosts[i].ObjectA == _currency)
            {
                totalCost += activationCosts[i].ObjectB;
            }
        }


        switch (_currency)
        {

        }
    }
    */





    public virtual bool IsUsable
    {
        get { return gameObject.activeInHierarchy; }
    }
    public abstract bool IsUsableOutsideFOV { get; }



    public virtual void InflateUI()
    {
        if (activeUI != null && activeUI.gameObject.activeInHierarchy && activeUI.TargetTransform == m_Transform)
        {
            activeUI.SetFollowOffset(Vector3.zero);
        }
        else
        {
            if (ObjectPoolerManager.Instance == null)
                return;


            bool hasUIInflated = false;

            GameObject uiObj = ObjectPoolerManager.Instance.InteractableUIPooler.GetPooledObject();

            if (uiObj == null)
                return;

            if (m_Transform == null)
                m_Transform = GetComponent<Transform>();


            activeUI = uiObj.GetComponent<GenericUI>();

            uiObj.transform.position = transform.position;
            uiObj.SetActive(true);
            activeUI.Initialize(m_Transform, shouldShowLines);



            GameObject _text;// = activeUI.GetPrefab("ID");

            if (!string.IsNullOrEmpty(Name) && (_text = activeUI.GetPrefab("ID") ) != null)
            {
                DisplayUI _ui = _text.GetComponent<DisplayUI>();
                _text.SetActive(!string.IsNullOrEmpty(Name));


                activeUI.AddAttribute(new GenericUI.DisplayProperties("ID", new Orientation(m_UIOffset, Vector3.zero, Vector3.one), _ui));
                activeUI.UpdateAttribute("ID", Name);

                hasUIInflated = true;
            }

            bool hasCost = false;
            for(int i = 0; i < ActivationCosts.Count; i++)
            {
                if(ActivationCosts[i].Value != 0)
                {
                    hasCost = true;
                    break;
                }
            }


            Transform _transform = activeUI.GetParentTransform("Charges");

            if (_transform != null)
            {
                _transform.gameObject.SetActive(hasCost);
            }



            if (hasCost)
            {
                //if (Utilities.HasFlag(m_ActivationCosts.Currency, CurrencyType.Experience))
                //{
                //    AddCostUI(CurrencyType.Experience, "Experience", m_ActivationCosts.ExperienceCost);
                //}

                //if (Utilities.HasFlag(m_ActivationCosts.Currency, CurrencyType.Health))
                //{
                //    AddCostUI(CurrencyType.Health, "Health", m_ActivationCosts.HealthCost);
                //}

                //if (Utilities.HasFlag(m_ActivationCosts.Currency, CurrencyType.LevelPoints))
                //{
                //    AddCostUI(CurrencyType.LevelPoints, "Level Points", m_ActivationCosts.LevelPointCost);
                //}

                //if (Utilities.HasFlag(m_ActivationCosts.Currency, CurrencyType.StatLevel))
                //{
                //    List<Tuple_StatTypeInt> statList = m_ActivationCosts.StatLevelCost;

                //    for(int i = 0; i < statList.Count; i++)
                //    {
                //        AddCostUI(CurrencyType.StatLevel, statList[i].Item1.ToString(), statList[i].Item2);
                //    }
                //}


                for (int i = 0; i < ActivationCosts.Count; i++)
                {
                    if (ActivationCosts[i].Currency == CurrencyType.StatLevel)
                    {
                        AddCostUI(ActivationCosts[i].Currency, ActivationCosts[i].StatType.ToString(), ActivationCosts[i].Value);
                    }
                    else
                    {
                        AddCostUI(ActivationCosts[i].Currency, ActivationCosts[i].Currency.ToString(), m_ActivationCosts[i].Value);
                    }

                    //    GameObject costUI = activeUI.GetPrefab("Cost");

                    //    if (costUI == null)
                    //        continue;



                    //    ProgressBarController controller = costUI.GetComponent<ProgressBarController>();



                    //    Color uiColor = Color.white;
                    //    uiColor = ColorManager.GetColor(ActivationCosts[i].Currency);

                    //    controller.Color = uiColor;
                    //    controller.SetText(activationCosts[i].Item2.ToString());

                    //    costUI.SetActive(true);


                    //    activeUI.AddAttribute(new GenericUI.DisplayProperties(activationCosts[i].Item1.ToString(), new Orientation(Vector3.zero, Vector3.zero, Vector3.one), controller), activationCosts[i].Item2 < 0 ? "Cost" : "Reward");
                    //}
                }


                hasUIInflated = true;
            }


            if (OnUI_Inflate != null)
            {
                OnUI_Inflate(m_UIComponents);
            }


            if (hasUIInflated)
            {
                SoundClip _uiSound = SoundManager.Instance.UI_Sound;
                if (m_Audio != null && _uiSound.Sound != null)
                {
                    m_Audio.Stop();

                    m_Audio.volume = _uiSound.Volume;
                    m_Audio.pitch = _uiSound.Pitch;
                    m_Audio.PlayOneShot(_uiSound.Sound);
                }
            }
        }
    }
    public virtual void DeflateUI()
    {
        if (activeUI == null || !activeUI.gameObject.activeInHierarchy || activeUI.TargetTransform != m_Transform)
            return;
        
        activeUI.Deflate();

        if (OnUI_Deflate != null)
        {
            OnUI_Deflate(m_UIComponents);
        }
    }

   
    private void AddCostUI(CurrencyType c, string name, int amount)
    {
            GameObject costUI = activeUI.GetPrefab("Cost");

            if (costUI == null)
                return;



            ProgressBarController controller = costUI.GetComponent<ProgressBarController>();



            Color uiColor = ColorManager.GetColor(c);

            controller.Color = uiColor;
            controller.SetText(amount.ToString());

            costUI.SetActive(true);


            activeUI.AddAttribute(new GenericUI.DisplayProperties(name, new Orientation(Vector3.zero, Vector3.zero, Vector3.one), controller), amount < 0 ? "Cost" : "Reward");
 
    }


    protected virtual void OnTriggerEnter(Collider coll)
    {
        PlayerController pController = coll.GetComponent<PlayerController>();
        if (!coll.isTrigger && pController != null)
        {
            activatingObjects.Add(coll.gameObject);

        }
    }
    protected virtual void OnTriggerExit(Collider coll)
    {
        if (!coll.isTrigger)
        {
            activatingObjects.Remove(coll.gameObject);
        }
    }

    protected virtual void PlaySound(SoundClip _sound)
    {
        if (_sound.UseRemnant)
        {
            GameObject remnantObj = ObjectPoolerManager.Instance.AudioRemnantPooler.GetPooledObject();
            AudioRemnant remnantAudio = remnantObj.GetComponent<AudioRemnant>();

            remnantObj.SetActive(true);
            remnantAudio.PlaySound(_sound);
        }
        else
        {
            m_Audio.volume = _sound.Volume;
            m_Audio.pitch = _sound.Pitch;

            if (_sound.IsLooping)
            {
                m_Audio.loop = true;
                m_Audio.clip = _sound.Sound;
                m_Audio.Play();
            }
            else
            {
                m_Audio.loop = false;
                m_Audio.PlayOneShot(_sound.Sound);
            }
        }
    }

    #region Event Triggers

    protected void OnUseTrigger()
    {
        if (OnUse != null)
        {
            ObjectAcquired(this, EventArgs.Empty);
            OnUse();
        }
    }

    protected void OnGiveTrigger()
    {
        if (OnGive != null)
        {
            OnGive();
        }
    }

   
    #endregion


    protected void OnObjectAcquired()
    {
        ObjectAcquired(this, EventArgs.Empty);
    }




    public string Name
    {
        get { return m_ObjectName; }
        set { m_ObjectName = value; }
    }

    public GameObject Object
    {
        get
        {
            return gameObject;
        }
    }
    protected List<Cost> ActivationCosts
    {
        get { return m_ActivationCosts; }
    }
}
