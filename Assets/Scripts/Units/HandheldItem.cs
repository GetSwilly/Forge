using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public abstract class HandheldItem : MonoBehaviour, IIdentifier {

    protected static readonly float DEBUG_DRAW_TIME = 2f;


    [SerializeField]
    string m_ItemName = "Handheld Item";

    [SerializeField]
    protected bool showDebug = false;



    [Space(10)]
    [Header("Input Types")]
    [Space(5)]

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

    
    [Space(10)]
    [Header("Movement Effects")]
    [Space(5)]


    
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


    [Space(10)]
    [Header("Camera Shake")]
    [Space(5)]

    [Tooltip("Camera Shake to apply on PRIMARY action activation")]
    [SerializeField]
    [Range(0f, 2f)]
    float shakeAmountPrimary = 0.2f;

    [Tooltip("Camera Shake to apply on SECONDARY action activation")]
    [SerializeField]
    [Range(0f, 2f)]
    float shakeAmountSecondary = 0.2f;

    [Tooltip("Camera Shake to apply on TERTIARY action activation")]
    [SerializeField]
    [Range(0f, 2f)]
    float shakeAmountTertiary = 0.2f;

    [Tooltip("Camera Shake time to apply on all activations")]
    [SerializeField]
    [Range(0f, 1f)]
    float shakeTime = 0f;


    [Space(15)]
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



    private Dictionary<StatType, Stat> m_StatDictionary = new Dictionary<StatType, Stat>();

    public delegate void AlertEvent();
    public event AlertEvent OnActivatePrimary;
    public event AlertEvent OnActivateSecondary;
    public event AlertEvent OnActivateUtility;

    public delegate void WeaponChange(float _percent, bool setImmediate);
    public event WeaponChange OnWeaponChanged;

    public delegate void WeaponCasualty(Health casualtyHealth);
    public event WeaponCasualty OnWeaponCasualty;

    protected Transform m_Transform;
    protected Transform m_Owner;
    protected AudioSource myAudio;

    protected MovementController m_Movement;


    public virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        myAudio = GetComponent<AudioSource>();
    }



    public virtual void Initialize(Transform _transform, List<Stat> _stats)
    {
        AlertWeaponChange(GetPercentage(), false);
        //SetVolume(1);

        m_Owner = _transform;
        UpdateStats(_stats);


        m_Movement = _transform.GetComponent<MovementController>();
        if(m_Movement != null)
        {
            m_Movement.AddSpeedMultiplier(SpeedMultiplier);
            m_Movement.AddRotationMultiplier(RotationMultiplier);
        }
    }
    public virtual void Terminate()
    {
        GameObject g = GameObject.Find("Generated Objects");
        m_Transform.parent = g == null ? null : g.transform;

        OnActivatePrimary = null;
        OnActivateSecondary = null;
        OnActivateUtility = null;

        OnWeaponChanged = null;
        OnWeaponCasualty = null;

        m_Owner = null;

        if (m_Movement != null)
        {
            m_Movement.RemoveSpeedMultiplier(SpeedMultiplier);
            m_Movement.RemoveRotationMultiplier(RotationMultiplier);
        }
        m_Movement = null;

        m_StatDictionary.Clear();
    }




    public void UpdateStats(List<Stat> _stats)
    {
        _stats.ForEach(s => UpdateStat(s));
    }
    public abstract void UpdateStat(Stat _stat);


    public abstract void ActivatePrimary();
    public abstract void DeactivatePrimary();
    public abstract bool CanActivatePrimary();

    public abstract void ActivateSecondary();
    public abstract void DeactivateSecondary();
    public abstract bool CanActivateSecondary();

    public abstract void ActivateTertiary();
    public abstract void DeactivateTertiary();
    public abstract bool CanActivateTertiary();



    public abstract void EnableEffects();
    public abstract void DisableEffects();

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
    protected void AlertWeaponChange(float _percent, bool setImmediate)
    {
        if (OnWeaponChanged != null)
        {
            OnWeaponChanged(_percent, setImmediate);
        }
    }
    protected void AlertWeaponCasualty(Health casualtyHealth)
    {
        if (OnWeaponCasualty != null)
            OnWeaponCasualty(casualtyHealth);
    }



    protected virtual void PlaySound(SoundClip _sound)
    {
        //Debug.Log("Playing Sound : " + _sound);

        myAudio.volume = _sound.Volume;
        myAudio.pitch = _sound.Pitch;

        if (_sound.IsLooping)
        {
            myAudio.loop = true;
            myAudio.clip = _sound.Sound;
            myAudio.Play();
        }
        else
        {
            myAudio.loop = false;
            myAudio.PlayOneShot(_sound.Sound);
        }
    }


    protected Stat GetStat(StatType _type)
    {
        return m_StatDictionary.ContainsKey(_type) ? m_StatDictionary[_type] : null;
    }
    protected float GetStatValue(StatType _type)
    {
        Stat _stat = GetStat(_type);

        return _stat == null ? 0f : _stat.CurrentValue;
    }

    #region Accessors

    public string Name
    {
        get { return m_ItemName; }
        set { m_ItemName = value; }
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



    public float ShakeAmountPrimary
    {
        get { return shakeAmountPrimary; }
    }
    public float ShakeAmountSecondary
    {
        get { return shakeAmountSecondary; }
    }
    public float ShakeAmountTertiary
    {
        get { return shakeAmountTertiary; }
    }
    public float ShakeTime
    {
        get { return shakeTime; }
    }

    #endregion


    public override string ToString()
    {
        return m_ItemName;
    }

}
