using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForgeSite : MonoBehaviour, IMemorable, ITeamMember, IStat
{
    [SerializeField]
    Team m_Team;

    [SerializeField]
    List<GameObject> m_PossibleForges = new List<GameObject>();


    [SerializeField]
    GameObject activeForge;

    [SerializeField]
    bool isLockedIn = false;

    [SerializeField]
    Transform forgeOrigin;

    [SerializeField]
    UnitStats m_Stats;

    Transform m_Transform;

    public event Delegates.StatChanged OnLevelChanged;


    void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }
    void Start()
    {
        if (activeForge != null)
        {
            Forge(activeForge.GetComponent<IForgeable>());
        }
    }

    public void Forge(IForgeable forgeObj)
    {
        Forge(forgeObj, null);
    }
    public void Forge(IForgeable forgeObj, ITeamMember _member)
    {
        if (forgeObj == null)
            return;
        
        activeForge = forgeObj.GameObject;

        activeForge.transform.SetParent(forgeOrigin == null ? m_Transform : forgeOrigin);
        activeForge.transform.localPosition = Vector3.zero;
        activeForge.transform.localRotation = Quaternion.identity;
        // activeForge.transform.localScale = Vector3.one;

        forgeObj.Initialize(this, _member);
    }
    public void RemoveActiveForge()
    {
        if (activeForge == null)
            return;

        GameObject g = activeForge;

        activeForge = null;

        Destroy(g);
    }
    public bool CanRemoveActive()
    {
        return !isLockedIn;
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

    #region Stats

    protected Stat GetStat(StatType _type)
    {
        return m_Stats.GetStat(_type);
    }


    public int GetCurrentStatLevel(StatType _type)
    {
        return m_Stats.GetCurrentStatLevel(_type);
    }
    public int GetMaxStatLevel(StatType _type)
    {
        return m_Stats.GetMaxStatLevel(_type);
    }
    public bool HasStat(StatType _type)
    {
        return m_Stats.HasStat(_type);
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

    public List<IForgeable> Forgeables
    {
        get
        {
            List<IForgeable> _forgeables = new List<IForgeable>();
            m_PossibleForges.ForEach(p =>
            {
                IForgeable f = p.GetComponent<IForgeable>();

                if (f != null)
                {
                    _forgeables.Add(f);
                }
            });

            return _forgeables;
        }
    }
    //public bool IsLockedIn
    //{
    //    get { return isLockedIn; }
    //}

    #endregion
}
