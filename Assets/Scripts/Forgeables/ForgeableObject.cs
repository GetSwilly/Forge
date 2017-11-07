using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Team))]
public abstract class ForgeableObject : MonoBehaviour, IMemorable
{

    [SerializeField]
    protected bool showDebug = false;

    [SerializeField]
    string objectName;

    [SerializeField]
    protected Transform m_Pivot;

    [SerializeField]
    float buildTime;

    [SerializeField]
    protected CustomRangeSensor m_Sight;

    [SerializeField]
    float range = 10f;

    [SerializeField]
    protected bool affectFriendly;

    [SerializeField]
    protected bool affectEnemy;

    [SerializeField]
    protected bool affectUnclassified;

    protected Team m_Team;
    protected Transform m_Transform;

    UnityAction<GameObject> onSightAction;
    UnityAction<GameObject> onMaintainSightAction;
    UnityAction<GameObject> onLostSightAction;


    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();

        m_Team = GetComponent<Team>();
    }
    protected virtual void Start()
    {
        onSightAction += SightGained;
        onMaintainSightAction += SightMaintained;
        onLostSightAction += SightLost;

        AddSightCollider();
    }
    protected virtual void OnEnable()
    {
        m_Pivot.localRotation = Quaternion.identity;
    }

    void AddSightCollider()
    {
        m_Sight.OnDetected.AddListener(onSightAction);
        m_Sight.OnStayDetected.AddListener(onMaintainSightAction);
        m_Sight.OnLostDetection.AddListener(onLostSightAction);
    }

    public virtual void Initialize(ForgeSite forgeActivator)
    {
        Initialize(forgeActivator, null);
    }

    public virtual void Initialize(ForgeSite forgeActivator, Team team)
    {
        if (team != null)
        {
            m_Team.TeamType = team.TeamType;
            m_Team.FriendlyTeams = team.FriendlyTeams;
            m_Team.FriendlyTeams.Add(Team.GetTeam(team.TeamType));
            m_Team.EnemyTeams = team.EnemyTeams;
        }
    }

    public bool ShouldAffect(Team team)
    {
        bool isFriendly = m_Team.IsFriendly(m_Team);
        bool isEnemy = m_Team.IsEnemy(m_Team);

        return (affectFriendly && isFriendly)
             || (affectEnemy && isEnemy)
             || (affectUnclassified && !(isFriendly || isEnemy));
    }

    protected virtual void SightGained(GameObject obj) { }
    protected virtual void SightMaintained(GameObject obj) { }
    protected virtual void SightLost(GameObject obj) { }


    #region Accessors

    public string Name
    {
        get { return objectName; }
    }
    public Transform Transform
    {
        get { return m_Transform; }
    }
    public GameObject GameObject
    {
        get { return this.gameObject; }
    }

    public float BuildTime
    {
        get { return buildTime; }
        private set { buildTime = Mathf.Clamp(value, 0f, value); }
    }

    public float Range
    {
        get { return range; }
        private set
        {
            range = Mathf.Clamp(value, 0f, value);

            if (m_Sight != null)
            {
                m_Sight.SensorRange = Range;
            }
        }
    }
    #endregion

    protected virtual void OnValidate()
    {
        BuildTime = BuildTime;
        Range = Range;

        if(m_Sight != null)
        {
            m_Sight.OnValidate();
        }
    }
}
