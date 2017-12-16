using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Identifier))]
[RequireComponent(typeof(Team))]
public abstract class HandheldItem : MonoBehaviour
{
    protected static readonly float DEBUG_DRAW_TIME = 2f;

    
    [SerializeField]
    protected bool showDebug = false;


    /*
    *******************************************************************************************************************************
    */
    [Header("Input Types")]

    [Tooltip("Input type for PRIMARY action")]
    [SerializeField]
    [EnumFlags]
    protected InputType primaryInputType = InputType.Hold;

    [Tooltip("Input type for SECONDARY action")]
    [SerializeField]
    [EnumFlags]
    protected InputType secondaryInputType = InputType.Hold;

    [Tooltip("Input type for TERTIARY action")]
    [SerializeField]
    [EnumFlags]
    protected InputType tertiaryInputType = InputType.Hold;


    /*
     *******************************************************************************************************************************
     */
    [Header("Movement Effects")]


    [Tooltip("Speed Multiplier to apply on Item pickup")]
    [SerializeField]
    [Range(0f, 1f)]
    float generalSpeedSpeedup = 1f;

    [Tooltip("Rotation Multiplier to apply on Item pickup")]
    [SerializeField]
    [Range(0f, 1f)]
    float generalRotationSpeedup = 1f;

    [Tooltip("Speed Multiplier to apply on PRIMARY action activation")]
    [SerializeField]
    [Range(0f, 1f)]
    protected float primarySpeedSpeedup = 1f;

    [Tooltip("Rotation Multiplier to apply on PRIMARY action activation")]
    [SerializeField]
    [Range(0f, 1f)]
    protected float primaryRotationSpeedup = 1f;

    [Tooltip("Speed Multiplier to apply on SECONDARY action activation")]
    [SerializeField]
    [Range(0f, 1f)]
    protected float secondarySpeedSpeedup = 1f;

    [Tooltip("Rotation Multiplier to apply on SECONDARY action activation")]
    [SerializeField]
    [Range(0f, 1f)]
    protected float secondaryRotationSpeedup = 1f;

    [Tooltip("Speed Multiplier to apply on TERTIARY action activation")]
    [SerializeField]
    [Range(0f, 1f)]
    protected float tertiarySpeedSpeedup = 1f;

    [Tooltip("Rotation Multiplier to apply on TERTIARY action activation")]
    [SerializeField]
    [Range(0f, 1f)]
    protected float tertiaryRotationSpeedup = 1f;


    //[Space(10)]
    //[Header("Camera Shake")]
    //[Space(5)]

    //[Tooltip("Camera Shake to apply on PRIMARY action activation")]
    //[SerializeField]
    //[Range(0f, 2f)]
    //float shakeAmountPrimary = 0.2f;

    //[Tooltip("Camera Shake to apply on SECONDARY action activation")]
    //[SerializeField]
    //[Range(0f, 2f)]
    //float shakeAmountSecondary = 0.2f;

    //[Tooltip("Camera Shake to apply on TERTIARY action activation")]
    //[SerializeField]
    //[Range(0f, 2f)]
    //float shakeAmountTertiary = 0.2f;

    //[Tooltip("Camera Shake time to apply on all activations")]
    //[SerializeField]
    //[Range(0f, 1f)]
    //float shakeTime = 0f;

    /*
    *******************************************************************************************************************************
    */
    [Header("Sounds")]
    [Space(5)]

    [Tooltip("Possible sounds for PRIMARY Action")]
    [SerializeField]
    protected List<WeightedObjectOfSoundClip> primaryActionSounds = new List<WeightedObjectOfSoundClip>();

    [Tooltip("Possible sounds for SECONDARY Action")]
    [SerializeField]
    protected List<WeightedObjectOfSoundClip> secondaryActionSounds = new List<WeightedObjectOfSoundClip>();

    [Tooltip("Possible sounds for TERTIARY Action")]
    [SerializeField]
    protected List<WeightedObjectOfSoundClip> tertiaryActionSounds = new List<WeightedObjectOfSoundClip>();

    [SerializeField]
    StatSubscriptions m_StatSubscriptions;

    
    public event Delegates.Alert OnActivatePrimary;
    public event Delegates.Alert OnActivateSecondary;
    public event Delegates.Alert OnActivateUtility;
    
    public event Delegates.ValueAlertEvent OnHandheldUpdate;
    public event Health.AlertHealthChange OnCauseHealthChange;

    protected Transform m_Transform;
    protected Transform m_Owner;
    protected AudioSource m_Audio;

    protected Team m_Team;
    protected MovementController m_Movement;

    Dictionary<StatType, int> statLevelTracker = new Dictionary<StatType, int>();

    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Audio = GetComponent<AudioSource>();
        m_Team = GetComponent<Team>();
    }


    public virtual void Initialize(Transform owner, Team team)
    {
        AlertHandheldUpdate(GetPercentage());
        //SetVolume(1);

        m_Owner = owner;

        m_Team.Copy(team);

        statLevelTracker.Clear();

        IStat statOwner = m_Owner.GetComponent<IStat>();
        if (statOwner != null)
        {
            statOwner.OnStatLevelChanged += UpdateStatEffect;
            InitializeStats(statOwner);
        }

        m_Movement = m_Owner.GetComponent<MovementController>();
        if (m_Movement != null)
        {
            m_Movement.AddSpeedMultiplier(this, SpeedMultiplier);
            m_Movement.AddRotationMultiplier(this, RotationMultiplier);
        }
    }
    public virtual void Terminate()
    {
        GameObject g = GameObject.Find("Generated Objects");
        m_Transform.parent = g == null ? null : g.transform;

        OnActivatePrimary = null;
        OnActivateSecondary = null;
        OnActivateUtility = null;

        OnHandheldUpdate = null;
        OnCauseHealthChange = null;

        if (m_Owner != null)
        {
            IStat statOwner = m_Owner.GetComponent<IStat>();
            if (statOwner != null)
            {
                statOwner.OnStatLevelChanged -= UpdateStatEffect;
            }
        }

        m_Owner = null;

        if (m_Movement != null)
        {
            m_Movement.RemoveSpeedMultiplier(this);
            m_Movement.RemoveRotationMultiplier(this);
        }
        m_Movement = null;

        m_Team.ResetTeam();
    }

    public abstract void ActivatePrimary();
    public abstract void DeactivatePrimary();
    public abstract bool CanActivatePrimary();

    public abstract void ActivateSecondary();
    public abstract void DeactivateSecondary();
    public abstract bool CanActivateSecondary();

    public abstract void ActivateTertiary();
    public abstract void DeactivateTertiary();
    public abstract bool CanActivateTertiary();

    public abstract float GetPercentage();
    
    protected void AlertPrimaryActivation()
    {
        if (OnActivatePrimary != null)
            OnActivatePrimary();
    }
    protected void AlertSecondaryActivation()
    {
        if (OnActivateSecondary != null)
            OnActivateSecondary();
    }
    protected void AlertUtilityActivation()
    {
        if (OnActivateUtility != null)
            OnActivateUtility();
    }
    protected void AlertHandheldUpdate(float _percent)
    {
        if (OnHandheldUpdate != null)
        {
            OnHandheldUpdate(_percent);
        }
    }
    protected void AlertHealthChangeCaused(Health casualtyHealth)
    {
        if (OnCauseHealthChange != null)
            OnCauseHealthChange(casualtyHealth);
    }



    protected virtual void PlaySound(SoundClip sound)
    {
        GameObject remnantObject = ObjectPoolerManager.Instance.AudioRemnantPooler.GetPooledObject();
        AudioRemnant remnantScript = remnantObject.GetComponent<AudioRemnant>();

        if (remnantScript != null)
        {
            remnantObject.transform.position = m_Transform.position;
            remnantObject.SetActive(true);

            remnantScript.PlaySound(sound);
        }

        //m_Audio.volume = sound.Volume;
        //m_Audio.pitch = sound.Pitch;

        //if (sound.IsLooping)
        //{
        //    m_Audio.loop = true;
        //    m_Audio.clip = sound.Sound;
        //    m_Audio.Play();
        //}
        //else
        //{
        //    m_Audio.loop = false;
        //    m_Audio.PlayOneShot(sound.Sound);
        //}
    }


    #region Stats

    protected float GetStatValue(StatType type)
    {
        if (!statLevelTracker.ContainsKey(type))
        {
            return 0f;
        }

        int level = statLevelTracker[type];
        return m_StatSubscriptions.GetValue(type, level);
    }
    void InitializeStats(IStat statOwner)
    {
        StatType[] statTypes = Enum.GetValues(typeof(StatType)) as StatType[];
        for (int i = 0; i < statTypes.Length; i++)
        {
            UpdateStatEffect(statTypes[i], statOwner.GetCurrentStatLevel(statTypes[i]));
        }
    }
    protected virtual void UpdateStatEffect(StatType type, int level)
    {
        if (statLevelTracker.ContainsKey(type))
        {
            statLevelTracker[type] = level;
        }
        else
        {
            statLevelTracker.Add(type, level);
        }
    }

    #endregion

    #region Accessors

    public string Name
    {
        get { return GetComponent<Identifier>().Name; }
    }
    public GameObject GameObject
    {
        get { return this.gameObject; }
    }
    public Transform Transform
    {
        get { return m_Transform; }
    }

    public InputType PrimaryInputType
    {
        get { return primaryInputType; }
    }
    public InputType SecondaryInputType
    {
        get { return secondaryInputType; }
    }
    public InputType TertiaryInputType
    {
        get { return tertiaryInputType; }
    }


    public float SpeedMultiplier
    {
        get { return generalSpeedSpeedup; }
    }
    public float RotationMultiplier
    {
        get { return generalRotationSpeedup; }
    }



    //public float ShakeAmountPrimary
    //{
    //    get { return shakeAmountPrimary; }
    //}
    //public float ShakeAmountSecondary
    //{
    //    get { return shakeAmountSecondary; }
    //}
    //public float ShakeAmountTertiary
    //{
    //    get { return shakeAmountTertiary; }
    //}
    //public float ShakeTime
    //{
    //    get { return shakeTime; }
    //}

    #endregion


    protected virtual void OnValidate()
    {
        if (m_StatSubscriptions != null)
        {
            m_StatSubscriptions.Validate();
        }
    }
}
