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
    //Team m_Team;

    InteractableObject m_Interactable;

    PlayerController forgingPlayer;
    ForgeableObject attemptedForge;
    GameObject timerUI;

    public event Delegates.StatChanged OnStatLevelChanged;


    void Awake()
    {
        m_Transform = GetComponent<Transform>();

        m_Interactable = GetComponent<InteractableObject>();
        //m_Team = GetComponent<Team>();
    }
    void Start()
    {
        if (activeForge != null)
        {
            Forge(null, activeForge.GetComponent<ForgeableObject>());
        }
    }

    public void Forge(PlayerController forger, ForgeableObject forgeObj)
    {
        Forge(forger, forgeObj, null);
    }
    public void Forge(PlayerController forger, ForgeableObject forgeObj, Team _team)
    {
        StartCoroutine(AttemptForge(forger, forgeObj, _team));
    }
    IEnumerator AttemptForge(PlayerController player, ForgeableObject forgeObj, Team team)
    {
        forgingPlayer = player;
        attemptedForge = forgeObj;
        m_Interactable.enabled = false;

        forgeObj.gameObject.SetActive(false);
        forgeObj.transform.position = m_Transform.position;

        timerUI = ObjectPoolerManager.Instance.TimerUIPooler.GetPooledObject();
        timerUI.SetActive(true);

        FollowTarget follow = timerUI.GetComponent<FollowTarget>();
        timerUI.transform.position = m_Transform.position;
        follow.SetTarget(m_Transform);

        ProgressBarController progress = timerUI.GetComponent<ProgressBarController>();
        progress.SetPercentage(1f, true);

        float timer = forgeObj.BuildTime;

        while (timer > 0f)
        {
            yield return null;
            timer -= Time.deltaTime;

            progress.SetPercentage(timer / forgeObj.BuildTime, false);
            string timerString = (Mathf.Round(timer * 100) / 100f).ToString();
            progress.SetText(timerString);
        }

        timerUI.SetActive(false);

        attemptedForge = null;
        timerUI = null;

        ProcessForge(forgeObj, team);

        m_Interactable.enabled = true;
    }
    void ProcessForge(ForgeableObject forgeObj, Team _team)
    {
        if (forgeObj == null)
            return;

        if (activeForge != null)
        {
            activeForge.transform.SetParent(null);
            activeForge.SetActive(false);
            Destroy(activeForge);
        }

        activeForge = forgeObj.gameObject;

        activeForge.transform.SetParent(forgeOrigin == null ? m_Transform : forgeOrigin);
        activeForge.transform.localPosition = Vector3.zero;
        activeForge.transform.localRotation = Quaternion.identity;
        // activeForge.transform.localScale = Vector3.one;

        forgeObj.gameObject.SetActive(true);

        forgeObj.Initialize(this, _team);
    }

    public void CancelForgeAttempt()
    {
        StopAllCoroutines();

        if (attemptedForge != null)
        {
            ItemPrice forgePrice = attemptedForge.GetComponent<ItemPrice>();

            if (forgePrice != null && forgingPlayer != null)
            {
                forgingPlayer.CreditArithmetic(forgePrice.CreditValue);
            }


            Destroy(attemptedForge.gameObject);
            attemptedForge = null;
        }

        if (timerUI != null)
        {
            timerUI.SetActive(false);
            timerUI = null;
        }

        m_Interactable.enabled = true;
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
        if (m_Stats != null)
        {
            m_Stats.Validate();
        }
    }
}
