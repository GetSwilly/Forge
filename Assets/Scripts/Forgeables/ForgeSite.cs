using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Team))]
public class ForgeSite : MonoBehaviour, IMemorable, IStat
{
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
    Team m_Team;

    public event Delegates.StatChanged OnLevelChanged;


    void Awake()
    {
        m_Transform = GetComponent<Transform>();

        m_Team = GetComponent<Team>();
    }
    void Start()
    {
        if (activeForge != null)
        {
            Forge(activeForge.GetComponent<ForgeableObject>());
        }
    }

    public void Forge(ForgeableObject forgeObj)
    {
        Forge(forgeObj, null);
    }
    public void Forge(ForgeableObject forgeObj, Team _team)
    {
        StartCoroutine(AttemptForge(forgeObj, _team));
    }
    IEnumerator AttemptForge(ForgeableObject forgeObj, Team team)
    {
        yield return new WaitForSeconds(forgeObj.BuildTime);
        ProcessForge(forgeObj, team);
    }
    void ProcessForge(ForgeableObject forgeObj, Team _team)
    {
        if (forgeObj == null)
            return;

        activeForge = forgeObj.gameObject;

        activeForge.transform.SetParent(forgeOrigin == null ? m_Transform : forgeOrigin);
        activeForge.transform.localPosition = Vector3.zero;
        activeForge.transform.localRotation = Quaternion.identity;
        // activeForge.transform.localScale = Vector3.one;

        forgeObj.Initialize(this, _team);
    }

    public void CancelForgeAttempt()
    {
        StopAllCoroutines();
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

    public List<ForgeableObject> Forgeables
    {
        get
        {
            List<ForgeableObject> _forgeables = new List<ForgeableObject>();
            m_PossibleForges.ForEach(p =>
            {
                ForgeableObject f = p.GetComponent<ForgeableObject>();

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

    void OnValidate()
    {
        m_Stats.Validate();
    }
}
