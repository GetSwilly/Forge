using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AttributeHandler))]
public abstract class UnitController : MonoBehaviour, IMemorable, ITeamMember, IStat{

    protected static readonly float PICKUP_MOVE_SPEED = 0.15f;
    protected static readonly float PICKUP_ROTATE_SPEED = 0.2f;
    protected static readonly float DROP_FORCE = 2f;


    [Tooltip("Should show debug information?")]
    [SerializeField]
    protected bool showDebug = false;

    [SerializeField]
    Team m_Team;
    
    [SerializeField]
    protected LayerMask ignoreMask;

    [Tooltip("GameObject tags to be considered friendly")]
    [SerializeField]
    protected List<string> friendlyTags = new List<string>();

    [Tooltip("GameObject tags to be considered friendly")]
    [SerializeField]
    protected List<string> allyTags = new List<string>();

    [SerializeField]
    protected LayerMask noHitMask;
    [SerializeField]
    protected LayerMask environmentMask;

    [SerializeField]
    protected UnitStats m_Stats;

    [SerializeField]
    StatSubscriptions m_StatSubscriptions;

    [SerializeField]
    private Transform headTransform;

    [Tooltip("Range of sight")]
    [SerializeField]
    private float m_SightRange = 8f;

    [Tooltip("Field of view")]
    [SerializeField]
    [Range(0, 360)]
    private int fieldOfView = 20;

    [Tooltip("Percentage of SightRange upon which unit takes notice of object")]
    [SerializeField]
    [Range(0f, 1f)]
    private float sightThreshold = .5f;

    [Tooltip("Range of hearing")]
    [SerializeField]
    private float hearingThreshold = 0f;


   protected bool isOperational = true;

    protected AttributeHandler m_Handler;
	protected Health m_Health;
	protected Transform m_Transform;
	protected AudioSource m_Audio;
    protected CharacterController m_Character;
    protected MovementController m_Movement;

    Dictionary<StatType, int> statLevelTracker = new Dictionary<StatType, int>();
    public event Delegates.StatChanged OnLevelChanged;


    public virtual void Awake()
    {
		m_Transform = GetComponent<Transform>();

        m_Handler = GetComponent<AttributeHandler>();
		m_Health = GetComponent<Health>();
		m_Audio = GetComponent<AudioSource>();
        m_Character = GetComponent<CharacterController>();
        m_Movement = GetComponent<MovementController>();
	}

	public virtual void Start()
    {
		if(GameManager.Instance != null)
        {
			//myHealth.OnDamaged += GameManager.Instance.UnitDamaged;
			//myHealth.OnKilled += GameManager.Instance.UnitKilled;
		}

        StatType[] sTypes = Enum.GetValues(typeof(StatType)) as StatType[];

        for (int i = 0; i < sTypes.Length; i++)
        {
            Stat s = m_Stats.GetStat(sTypes[i]);
            
            if (s == null)
                continue;

			s.OnValueChange += UpdateStatEffects;
			UpdateStatEffects(sTypes[i], s.CurrentLevel);
		}

        //Sight Setup
        if (headTransform != null)
        {
            SphereCollider sightCollider = this.gameObject.AddComponent<SphereCollider>();
            sightCollider.isTrigger = true;

            //Make Sight distance independent of scale
            Vector3 _scale = m_Transform.lossyScale;
            float maxVal = _scale.x;
            maxVal = maxVal > _scale.y ? maxVal : _scale.y;
            maxVal = maxVal > _scale.z ? maxVal : _scale.z;

            sightCollider.radius = m_SightRange / maxVal;
        }

        //Establish Health event listeners
        m_Health.OnHealthChange += UpdateUI;
        m_Health.OnDamaged += UnitDamaged;
        m_Health.OnKilled += UnitKilled;
    }

    public virtual void OnDisable()
    {
        m_Handler.RemoveAllActiveAttributes();

        m_Handler.HideUI();
    }


    

    /// <summary>
    /// Handle input from user
    /// </summary>
    public virtual void HandleInput(Vector3 moveVector, Vector3 aimPoint)
    {
        m_Movement.Move(m_Transform.position, moveVector,true);

        //Vector3 aimDirection = aimPoint - m_Transform.position;
        //m_Movement.RotateTowards(aimDirection);

        m_Movement.RotateTowards(aimPoint);
    }


    protected void PickupRune(Rune _rune)
    {
        if (_rune == null)
            return;

        StartCoroutine(PickupObject(_rune.transform, Vector3.zero, Quaternion.identity));
    }

    protected virtual void DropRune(Rune _rune)
    {
        if (_rune == null)
            return;
        
        _rune.Terminate();


        GameObject g = GameObject.Find("Generated Objects");
        _rune.transform.SetParent(null);

        if (g != null)
        {
            _rune.transform.SetParent(g.transform);
        }
        

        Rigidbody _rigidbody = _rune.GetComponent<Rigidbody>();
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(_rune.transform.forward * DROP_FORCE, ForceMode.Impulse);


        Utilities.SetRenderersEnabled(_rune.gameObject, true);
        Utilities.SetCollidersEnabled(_rune.gameObject, true);
    }

    protected IEnumerator PickupObject(Transform objTransform, Vector3 localPos, Quaternion localRotation)
    {
        yield return StartCoroutine(PickupObject(objTransform, localPos, localRotation, false, true));
    }
    protected IEnumerator PickupObject(Transform objTransform, Vector3 localPos, Quaternion localRotation, bool colliderStatus, bool rendererStatus)
    {

        Utilities.SetCollidersEnabled(objTransform, colliderStatus);

        Vector3 worldPos = m_Transform.TransformPoint(localPos);

        while ((objTransform.position - (worldPos = m_Transform.TransformPoint(localPos))).magnitude > 0.1f && Quaternion.Angle(objTransform.rotation, m_Transform.rotation) > 1f)
        {

            objTransform.position = Vector3.MoveTowards(objTransform.position, worldPos, PICKUP_MOVE_SPEED);

            objTransform.rotation = Quaternion.RotateTowards(objTransform.rotation, m_Transform.rotation, PICKUP_ROTATE_SPEED);

            yield return null;
        }

        objTransform.localPosition = localPos;
        objTransform.localRotation = localRotation;

        Utilities.SetRenderersEnabled(objTransform, rendererStatus);
    }




    /// <summary>
    /// Check if unit can see the provided transform
    /// </summary>
    public bool CanSee(Transform checkTransform)
    {
        return CanSee(checkTransform, FOV / 2f);
    }

    /// <summary>
    /// Check if unit can see the provided transform with the provided max sight angle
    /// </summary>
    /// <param name="checkTransform"></param>
    /// <param name="maxAngle"></param>
    /// <returns></returns>
    public bool CanSee(Transform checkTransform, float maxAngle)
    {
        Vector3 toVector = checkTransform.position - m_Transform.position;
        toVector.y = 0;

        if (toVector.magnitude > m_SightRange)
            return false;

        if (Vector3.Angle(toVector, m_Transform.forward) > maxAngle)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(new Ray(m_Transform.position, toVector), toVector.magnitude + 0.1f);

        for(int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].collider.isTrigger && hits[i].transform != checkTransform)
                return false;
        }

        return true;
    }  




    public abstract void NoiseHeard(AudioClip noise, Transform noiseOwner, Vector3 noisePosition, float noiseVolume);



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

    #region UI Stuff


    protected virtual void UpdateUI()
    {
        UpdateUI(m_Health);
    }
    protected void UpdateUI(Health _health)
    {
        m_Handler.UpdateUI(Attribute.Health, m_Health.HealthPercentage, false);
    }


    //public void UpdateAttributeUI()
    //{
    //    throw new NotImplementedException();
    //}

    //  protected void UpdateHealthProgressBar()
    //  {
    //      UpdateHealthProgressBar(m_Health);
    //  }
    //  protected void UpdateHealthProgressBar(Health healthScript)
    //  {
    //      if (m_UI == null || healthScript == null)
    //          return;

    //      m_UI.UpdateAttribute("Health", healthScript.HealthPercentage);
    //  }


    //  protected void UpdateHandheldProgressBar(float _percent, bool setImmediate)
    //  {
    //      if (m_UI == null)
    //          return;


    //      if (setImmediate)
    //      {
    //          m_UI.SetAttribute("Handheld", _percent);
    //      }
    //      else
    //      {
    //          m_UI.UpdateAttribute("Handheld", _percent);
    //      }
    //  }

    //  protected void UpdateAbilityProgressBar(float _percent, bool setImmediate)
    //  {
    //      if (m_UI == null)
    //          return;

    //      /*
    //if(setImmediate){
    //	UIManager.Instance.SetWeaponProgressBar(_percent);
    //}

    //UIManager.Instance.UpdateWeaponProgressBar(_percent);*/
    //  }

    //  protected void UpdateExpBar(float _percent)
    //  {
    //      if (m_UI == null)
    //          return;

    //      m_UI.UpdateAttribute("Experience", _percent);
    //  }



    //  public void UpdateAttributeUI(AttributeEffect _effect, float _percentage)
    //  {
    //      GetUI();

    //      m_UI.UpdateAttributeUI(_effect, _percentage);
    //  }

    #endregion



    public virtual bool CanAfford(Cost _cost)
    {
        switch (_cost.Currency)
        {
            case CurrencyType.Health:
                return m_Health.CanBeDamaged(_cost.Value);
            case CurrencyType.StatLevel:
                return m_Stats.CanChangeStatLevel(_cost.StatType, _cost.Value);
        }


        return true;
    }



    public void DamageAchieved(Health _casualtyHealth)
    {

    }
    public void KillAchieved(Health _casualtyHealth)
    {

    }
    public virtual void UnitDamaged(Health _casualtyHealth)
    {

    }
    public virtual void UnitKilled(Health _casualtyHealth)
    {
        m_Handler.HideUI();
    }


    #region Team
    public Team GetTeam()
    {
        return m_Team;
    }
    public SingleTeamClassification GetCurrentTeam()
    {
        return m_Team.CurrentTeam;
    }
    public TeamClassification[] GetFriendlyTeams()
    {
        return m_Team.FriendlyTeams;
    }
    public TeamClassification[] GetEnemyTeams()
    {
        return m_Team.EnemyTeams;
    }
    #endregion


    #region Accessors
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
        protected set { m_SightRange = Mathf.Clamp(value,0f,value); }
    }
    public float SightThreshold
    {
        get { return sightThreshold; }
        protected set { sightThreshold = Mathf.Clamp(value, 0f, value); }
    }
    public float HearingThreshold
    {
        get { return hearingThreshold; }
        protected set { hearingThreshold = Mathf.Clamp(value,0f,value); }
    }

    public LayerMask IgnoreMask
    {
        get { return ignoreMask; }
        set { IgnoreMask = value; }
    }
    public LayerMask NoHitMask
    {
        get { return noHitMask; }
    }
    public LayerMask EnvironmentMask
    {
        get { return environmentMask; }
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
            
            if(mStat != null && oStat != null)
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
        m_Stats.Validate();
        m_StatSubscriptions.Validate();

        SightRange = SightRange;
        SightThreshold = SightThreshold;
        HearingThreshold = HearingThreshold;
    }



    public virtual void OnDrawGizmos()
    {
		if(showDebug && m_Transform != null) {

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
        Gizmos.DrawLine(m_Transform.position, m_Transform.position + (leftRayDirection * m_SightRange * SightThreshold));
        Gizmos.DrawLine(m_Transform.position, m_Transform.position + (rightRayDirection * m_SightRange * SightThreshold));
    }
  
}
