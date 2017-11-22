using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Team))]
public abstract class UnitController : MonoBehaviour, IIdentifier, IMemorable, IStat
{

    protected static readonly float _PickupMovementSpeed = 15f;
    protected static readonly float _PickupRotateSpeed = 10f;
    protected static readonly float _PickupDropForce = 2f;

    [SerializeField]
    string unitName;

    [Tooltip("Should show debug information?")]
    [SerializeField]
    protected bool showDebug = false;

    [SerializeField]
    protected UnitStats m_Stats;

    [SerializeField]
    StatSubscriptions m_StatSubscriptions;

    [SerializeField]
    protected Vector3 rootPosition;

    [SerializeField]
    private CustomRangeSensor m_Sight;

    [Tooltip("Range of sight")]
    [SerializeField]
    private float m_SightRange = 8f;

    [Tooltip("Field of view")]
    [SerializeField]
    [Range(0, 360)]
    private int fieldOfView = 20;

    [SerializeField]
    private int m_Credits;


    protected bool isOperational = true;
    
    protected Health m_Health;
    protected Transform m_Transform;
    protected AudioSource m_Audio;
    protected Team m_Team;
    protected CharacterController m_Character;
    protected MovementController m_Movement;

    Dictionary<StatType, int> statLevelTracker = new Dictionary<StatType, int>();
    public event Delegates.StatChanged OnStatLevelChanged;

    private UnityAction<GameObject> onSightAction;
    private UnityAction<GameObject> onMaintainSightAction;
    private UnityAction<GameObject> onLostSightAction;

    public event Delegates.ValueChangeEvent OnHealthChange;
    public event Delegates.ValueChangeEvent OnCreditsChange;

    public event Health.AlertHealthChange OnDamageAchieved;

    public virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        
        m_Health = GetComponent<Health>();
        m_Audio = GetComponent<AudioSource>();
        m_Team = GetComponent<Team>();
        m_Character = GetComponent<CharacterController>();
        m_Movement = GetComponent<MovementController>();
    }

    public virtual void Start()
    {
        if (GameManager.Instance != null)
        {
            // m_Health.OnDamaged += GameManager.Instance.UnitDamaged;
            // m_Health.OnKilled += GameManager.Instance.UnitKilled;
        }


        onSightAction += SightGained;
        onMaintainSightAction += SightMaintained;
        onLostSightAction += SightLost;

        SubscribeToStats();

        AddSightCollider();

        //Establish Health event listeners
        m_Health.OnHealthChange += HealthChanged;
        m_Health.OnKilled += Died;
    }
    void SubscribeToStats()
    {
        StatType[] sTypes = Enum.GetValues(typeof(StatType)) as StatType[];

        for (int i = 0; i < sTypes.Length; i++)
        {
            Stat s = m_Stats.GetStat(sTypes[i]);

            if (s == null)
                continue;

            s.OnValueChange += UpdateStatEffects;
            UpdateStatEffects(sTypes[i], s.CurrentLevel);
        }
    }
    void AddSightCollider()
    {
        if (m_Sight == null)
            return;

        //SphereCollider sightCollider = m_Sight.GetComponent<SphereCollider>();

        //if (sightCollider == null)
        //{
        //    sightCollider = m_Sight.gameObject.AddComponent<SphereCollider>();
        //}
        //sightCollider.isTrigger = true;

        ////Make Sight distance independent of scale
        //Vector3 _scale = m_Transform.lossyScale;
        //float maxVal = _scale.x;
        //maxVal = maxVal > _scale.y ? maxVal : _scale.y;
        //maxVal = maxVal > _scale.z ? maxVal : _scale.z;

        //sightCollider.radius = m_SightRange / maxVal;


        m_Sight.OnDetected.AddListener(onSightAction);
        m_Sight.OnStayDetected.AddListener(onMaintainSightAction);
        m_Sight.OnLostDetection.AddListener(onLostSightAction);
        //CollisionNotifier sightNotifier = m_Sight.GetComponent<CollisionNotifier>();
        //if(sightNotifier != null)
        //{
        //    sightNotifier.CollisionEnter += SightCollisionEntered;
        //    sightNotifier.CollisionStay += SightCollisionStayed;
        //    sightNotifier.CollisionExit += SightCollisionExited;

        //    sightNotifier.TriggerEnter += SightTriggerEntered;
        //    sightNotifier.TriggerStay += SightTriggerStayed;
        //    sightNotifier.TriggerExit += SightTriggerExited;
        //}
    }




    /// <summary>
    /// Handle input from user
    /// </summary>
    public virtual void HandleInput(Vector3 moveVector, Vector3 aimPoint)
    {
        m_Movement.Move(m_Transform.position, moveVector, true);

        //Vector3 aimDirection = aimPoint - m_Transform.position;
        //m_Movement.RotateTowards(aimDirection);

        m_Movement.RotateTowards(aimPoint);
    }

    protected IEnumerator PickupObject(Transform objTransform, Vector3 localPos, Quaternion localRotation)
    {
        yield return StartCoroutine(PickupObject(objTransform, localPos, localRotation, false, true));
    }
    protected IEnumerator PickupObject(Transform objTransform, Vector3 localPos, Quaternion localRotation, bool colliderStatus, bool rendererStatus)
    {

        Utilities.SetCollidersEnabled(objTransform, colliderStatus);

        Vector3 worldPos = m_Transform.TransformPoint(localPos);

        while ((objTransform.position - (worldPos = m_Transform.TransformPoint(localPos))).magnitude > 0.01f)// && Quaternion.Angle(objTransform.rotation, m_Transform.rotation) > 1f)
        {
            yield return null;

            objTransform.position = Vector3.MoveTowards(objTransform.position, worldPos, _PickupMovementSpeed * Time.deltaTime);

            objTransform.rotation = Quaternion.RotateTowards(objTransform.rotation, m_Transform.rotation, _PickupRotateSpeed * Time.deltaTime);
        }

        objTransform.localPosition = localPos;
        objTransform.localRotation = localRotation;

        Utilities.SetRenderersEnabled(objTransform, rendererStatus);
    }



    public bool CanSee(Vector3 checkPosition)
    {
        return CanSee(checkPosition, FOV / 2f, null);
    }

    /// <summary>
    /// Check if unit can see the provided transform
    /// </summary>
    public bool CanSee(Transform checkTransform)
    {
        return CanSee(checkTransform, FOV / 2f);
    }

    public bool CanSee(Transform checkTransform, float maxAngle)
    {
        if (checkTransform == null)
            return false;

        return CanSee(checkTransform.position, FOV / 2f, new List<Transform>() { checkTransform });
    }

    /// <summary>
    /// Check if unit can see the provided transform with the provided max sight angle
    /// </summary>
    /// <param name="checkTransform"></param>
    /// <param name="maxAngle"></param>
    /// <returns></returns>
    public bool CanSee(Vector3 checkPosition, float maxAngle, List<Transform> validTransforms)
    {
        return false;

        //Vector3 toVector = checkPosition - headTransform.position;
        //toVector.y = 0;

        //if (toVector.magnitude > m_SightRange)
        //    return false;

        //if (Vector3.Angle(toVector, headTransform.forward) > maxAngle)
        //    return false;

        //RaycastHit[] hits = Physics.RaycastAll(new Ray(headTransform.position, toVector), toVector.magnitude + 0.1f);

        //for (int i = 0; i < hits.Length; i++)
        //{
        //    if (!hits[i].collider.isTrigger && (validTransforms != null && !validTransforms.Contains(hits[i].transform)))
        //    {
        //        return false;
        //    }
        //}

        //return true;
    }


    #region Stat Stuff

    public int GetCurrentStatLevel(StatType _type)
    {
        return m_Stats.GetCurrentStatLevel(_type);
    }

    /// <summary>
    /// Retrieve maximum possible level of Stat object corresponding to provided StatType
    /// </summary>
    public int GetMaxStatLevel(StatType _type)
    {
        return m_Stats.GetMaxStatLevel(_type);
    }
    public bool HasStat(StatType _type)
    {
        return m_Stats.HasStat(_type);
    }
    protected Stat GetStat(StatType _type)
    {
        return m_Stats.GetStat(_type);
    }

    public bool CanChangeStatLevel(StatType _type, int _delta)
    {
        return CanChangeStatLevel(_type, _delta, true);
    }
    public bool CanChangeStatLevel(StatType _type, int _delta, bool canOvershootMax)
    {
        Stat _stat = m_Stats.GetStat(_type);

        if (_stat != null)
        {
            return _stat.CanChangeLevel(_delta, canOvershootMax);
        }

        return false;
    }
    public void ChangeStat(StatType _type, int _delta)
    {
        m_Stats.ChangeStat(_type, _delta);
    }

    ///// <summary>
    ///// Check if capable of altering Stat object corresponding to provided StatType by provided delta
    ///// </summary>
    //public bool CanChangeStatLevel(StatType _type, int _delta)
    //{
    //    return CanChangeStatLevel(_type, _delta, true);
    //}
    //public bool CanChangeStatLevel(StatType _type, int _delta, bool canOvershootMax)
    //{
    //    Stat _stat = m_Stats.GetStat(_type);

    //    if (_stat != null)
    //    {
    //        return _stat.CanChangeLevel(_delta, canOvershootMax);
    //    }

    //    return false;
    //}

    //public bool HasStat(StatType _type)
    //{
    //    return m_Stats.HasStat(_type);
    //}
    //protected Stat GetStat(StatType _type)
    //{
    //    return m_Stats.GetStat(_type);
    //}
    //public void ChangeStat(StatType _type, int _delta)
    //{
    //    m_Stats.ChangeStat(_type, _delta);
    //}
    //public int GetStatLevel_Current(StatType _type)
    //{
    //    return m_Stats.GetStatLevel_Current(_type);
    //}

    ///// <summary>
    ///// Retrieve maximum possible level of Stat object corresponding to provided StatType
    ///// </summary>
    //public int GetStatLevel_Max(StatType _type)
    //{
    //    return m_Stats.GetStatLevel_Max(_type);
    //}

    ///// <summary>
    ///// Retrieve value associated with Stat object corresponding to provided StatType
    ///// </summary>
    //public float GetStatValue(StatType _type)
    //{

    //    Stat _stat = GetStat(_type);

    //    if (_stat != null)
    //        return _stat.CurrentValue;



    //    return 0;
    //}
    //protected void UpdateAllStatEffects()
    //{
    //    StatType[] _types = Enum.GetValues(typeof(StatType)) as StatType[];

    //    for (int i = 0; i < _types.Length; i++)
    //    {
    //        UpdateStatEffects(_types[i]);
    //    }
    //}

    protected virtual void UpdateStatEffects(StatType _type, int level)
    {
        float val = m_StatSubscriptions.GetValue(_type, level);

        switch (_type)
        {
            case StatType.Health:
                m_Health.AdditionalHealth = (int)val;
                break;
            case StatType.Speed:
                //if (m_Movement != null)
                //{
                //    m_Movement.AdditionalSpeedMultiplier = _stat.CurrentValue;
                //}
                break;
            default:
                break;
        }
    }

    #endregion


    #region Cost

    public virtual bool CanAfford(int creditsDelta)
    {
        return (Credits + creditsDelta) >= 0;
    }

    public bool CreditArithmetic(int creditsDelta)
    {
        if (!CanAfford(creditsDelta))
        {
            return false;
        }

        if (creditsDelta != 0)
        {
            UIManager.Instance.CreateDynamicInfoScript(transform.position, creditsDelta, Colors._CreditColor);
        }

        Credits += creditsDelta;


        if (OnCreditsChange != null)
        {
            OnCreditsChange(Credits, creditsDelta);
        }

        return true;
    }

    #endregion

    public virtual void UpdateUI()
    {
        CasualtyAchieved(null);
        HealthChanged(m_Health);
        CreditArithmetic(0);
    }

    public virtual void CasualtyAchieved(Health casualtyHealth)
    {
        if(OnDamageAchieved != null)
        {
            OnDamageAchieved(casualtyHealth);
        }
    }
    public void KillAchieved(Health casualtyHealth) { }
    public virtual void HealthChanged(Health mHealth)
    {
        if (mHealth != null && OnHealthChange != null)
        {
            OnHealthChange(mHealth.HealthPercentage, mHealth.LastHealthChange);
        }
    }
    public virtual void Died(Health mHealth) { }


    protected virtual void SightCollisionEntered(Collision coll) { }
    protected virtual void SightCollisionStayed(Collision coll) { }
    protected virtual void SightCollisionExited(Collision coll) { }

    protected virtual void SightTriggerEntered(Collider coll) { }
    protected virtual void SightTriggerStayed(Collider coll) { }
    protected virtual void SightTriggerExited(Collider coll) { }


    protected virtual void SightGained(GameObject obj) { }
    protected virtual void SightMaintained(GameObject obj) { }
    protected virtual void SightLost(GameObject obj) { }

    #region Accessors

    public String Name
    {
        get { return unitName; }
    }
    public GameObject GameObject
    {
        get { return this.gameObject; }
    }
    public Transform Transform
    {
        get { return m_Transform; }
    }

    public int FOV
    {
        get { return fieldOfView; }
        set { fieldOfView = value; }
    }
    public float SightRange
    {
        get { return m_SightRange; }
        protected set
        {
            m_SightRange = Mathf.Clamp(value, 0f, value);

            if (m_Sight != null)
            {
                m_Sight.SensorRange = SightRange * 2;
            }
        }
    }

    public bool ShowDebug
    {
        get { return showDebug; }
    }

    public bool IsOperational
    {
        get { return isOperational; }
        set { isOperational = value; }
    }

    protected int Credits
    {
        get { return m_Credits; }
        private set { m_Credits = Mathf.Clamp(value, 0, value); }
    }
    #endregion


    /// <summary>
    /// Copy properties of a provided UnitController
    /// </summary>
    public void Copy(UnitController other)
    {
        if (other == null)
            return;


        m_Transform.position = other.transform.position;
        m_Transform.rotation = other.transform.rotation;
        m_Transform.SetParent(other.transform.parent);

        CopyStatChanges(other);

    }
    public void CopyStatChanges(UnitController other)
    {
        StatType[] sTypes = Enum.GetValues(typeof(StatType)) as StatType[];

        for (int i = 0; i < sTypes.Length; i++)
        {
            Stat mStat = m_Stats.GetStat(sTypes[i]);
            Stat oStat = other.GetStat(sTypes[i]);

            if (mStat != null && oStat != null)
            {
                mStat.ModifyLevel(oStat.AddedLevels);
            }


            //if (_stat != null)
            //{
            //    //_stat = new Stat(m_Stats[i].Type, 0, m_Stats[i].MaxLevel, m_Stats[i].BaseValue, m_Stats[i].ValueChangePerLevel, m_Stats[i].Description);
            //    _stat = new Stat(m_Stats[i]);
            //    // _stat.ModifyLevel(m_Stats[i].AddedLevels);
            //}
            //else
            //{
            //    _stat.ModifyLevel(other.CharacterStats[i].AddedLevels);
            //}
        }
    }



    public virtual void OnValidate()
    {
        if (m_Stats != null)
        {
            m_Stats.Validate();
        }

        if (m_StatSubscriptions != null)
        {
            m_StatSubscriptions.Validate();
        }

        SightRange = SightRange;

        Credits = Credits;
    }



    public virtual void OnDrawGizmos()
    {
        if (showDebug && m_Transform != null)
        {

            DrawFOVGizmo();
        }
    }

    void DrawFOVGizmo()
    {
        Gizmos.color = Color.green;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-fieldOfView / 2f, m_Transform.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fieldOfView / 2f, m_Transform.up);
        Vector3 leftRayDirection = leftRayRotation * m_Transform.forward;
        Vector3 rightRayDirection = rightRayRotation * m_Transform.forward;
        Gizmos.DrawLine(m_Transform.position, m_Transform.position + (leftRayDirection * m_SightRange));
        Gizmos.DrawLine(m_Transform.position, m_Transform.position + (rightRayDirection * m_SightRange));
    }

}
