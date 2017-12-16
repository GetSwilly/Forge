using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Identifier))]
public abstract class InteractableObject : MonoBehaviour
{
    [SerializeField]
    bool isInteractable = true;
    
    [SerializeField]
    Vector3 m_UIOffset = new Vector3(0, 1, 2);
    
    [SerializeField]
    protected int activationCost;


    [Header("Sounds")]

    [SerializeField]
    protected SoundClip activationSound;



    [SerializeField]
    [EnumFlags]
    UIManager.Component m_UIComponents;

    
    public event Delegates.Alert OnUse;
    public event EventHandler ObjectAcquired;

    protected List<GameObject> activatingObjects = new List<GameObject>();


    UIBase activeUI;
    protected Transform m_Transform;
    protected Identifier m_Identifier;
    protected AudioSource m_Audio;



    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Identifier = GetComponent<Identifier>();
        m_Audio = GetComponent<AudioSource>();
        m_Audio.loop = false;
        m_Audio.playOnAwake = false;
    }
    void OnDisable()
    {
        DeflateUI();
    }


    public virtual bool Interact1(PlayerController controller)
    {
        if (activatingObjects.Count == 0)
            return false;


        bool isActivator = false;

        for (int i = 0; i < activatingObjects.Count; i++)
        {
            if (activatingObjects[i] == controller.gameObject)
                isActivator = true;
        }

        if (!isActivator)
            return false;

        
        if (!controller.CreditArithmetic(ActivationCost))
        {
            return false;
        }

        OnUseTrigger();

        PlaySound(activationSound);

        return true;
    }
    public virtual bool Interact2(PlayerController controller)
    {
        throw new NotImplementedException();
    }
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






    public virtual void InflateUI()
    {
        if (!this.enabled)
            return;

        if (activeUI == null || !activeUI.gameObject.activeInHierarchy || activeUI.Target != m_Transform)
        {
            if (ObjectPoolerManager.Instance == null)
                return;


            bool hasUIInflated = false;

            GameObject uiObj = ObjectPoolerManager.Instance.InteractableUIPooler.GetPooledObject();

            if (uiObj == null)
                return;

            if (m_Transform == null)
                m_Transform = GetComponent<Transform>();


            activeUI = uiObj.GetComponent<UIBase>();

            uiObj.transform.position = transform.position;
            uiObj.SetActive(true);
            activeUI.Inflate(m_Transform, m_Identifier.Name);


            hasUIInflated = true;

            if (hasUIInflated)
            {
                SoundClip uiSound = SoundManager.Instance.UI_Sound;
                if (m_Audio != null && uiSound.Sound != null)
                {
                    m_Audio.Stop();

                    m_Audio.volume = uiSound.Volume;
                    m_Audio.pitch = uiSound.Pitch;
                    m_Audio.PlayOneShot(uiSound.Sound);
                }
            }
        }
    }
    public virtual void DeflateUI()
    {
        if (activeUI == null || !activeUI.gameObject.activeInHierarchy || activeUI.Target != m_Transform)
            return;

        activeUI.Deflate();
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

    protected virtual void PlaySound(SoundClip sound)
    {
        if (sound.UseRemnant)
        {
            GameObject remnantObj = ObjectPoolerManager.Instance.AudioRemnantPooler.GetPooledObject();
            AudioRemnant remnantAudio = remnantObj.GetComponent<AudioRemnant>();

            remnantObj.SetActive(true);
            remnantAudio.PlaySound(sound);
        }
        else
        {
            m_Audio.volume = sound.Volume;
            m_Audio.pitch = sound.Pitch;

            if (sound.IsLooping)
            {
                m_Audio.loop = true;
                m_Audio.clip = sound.Sound;
                m_Audio.Play();
            }
            else
            {
                m_Audio.loop = false;
                m_Audio.PlayOneShot(sound.Sound);
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

    #endregion


    protected void OnObjectAcquired()
    {
        ObjectAcquired(this, EventArgs.Empty);
    }



    #region Accessors

    public virtual bool IsInteractable
    {
        get { return isInteractable; }
        protected set { isInteractable = value; }
    }
    public abstract bool IsUsableOutsideFOV { get; }


    public GameObject Object
    {
        get
        {
            return gameObject;
        }
    }
    protected int ActivationCost
    {
        get { return activationCost; }
        set { activationCost = Mathf.Clamp(value, 0, value); }
    }

    #endregion

    protected virtual void OnValidate()
    {
        ActivationCost = ActivationCost;
    }
}
