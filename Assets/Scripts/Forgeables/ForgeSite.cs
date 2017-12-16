using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Team))]
public class ForgeSite : MenuInflater, IMemorable, IStat
{
    [Header("Forge")]
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


    Health m_Health;
    PlayerController forgingPlayer;
    ForgeableObject processingForge;
    GameObject timerUI;
    Dictionary<MenuButton, ForgeableObject> buttonToForgeDictionary;

    public event Delegates.StatChanged OnStatLevelChanged;

    protected override void Awake()
    {
        base.Awake();

        m_Health = GetComponent<Health>();
    }
    void Start()
    {
        if (activeForge != null)
        {
            Forge(null, activeForge.GetComponent<ForgeableObject>());
        }

        buttonToForgeDictionary = new Dictionary<MenuButton, ForgeableObject>();
    }

    public override bool Interact1(PlayerController _player)
    {
        if (m_Health.NeedsHealth)
        {
            return false;
        }

        return base.Interact1(_player);
    }
    public override bool Interact2(PlayerController controller)
    {
        if (!m_Health.NeedsHealth)
            return false;

        float repairRate = controller.RepairSpeed * Time.deltaTime;
        m_Health.HealthArithmetic(repairRate, false, controller.transform);

        controller.CausedHealthChange(m_Health);

        return true;
    }

    #region Menu Inflation

    //public override bool Interact(PlayerController _player)
    //{
    //    return base.Interact(_player);
    //}

    public override void DeflateMenu()
    {
        base.DeflateMenu();

        Dictionary<MenuButton, ForgeableObject>.Enumerator enumerator = buttonToForgeDictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            Destroy(enumerator.Current.Key.gameObject);
        }

        buttonToForgeDictionary.Clear();
    }

    protected override void AddButtons()
    {
        buttonToForgeDictionary.Clear();

        List<ForgeableObject> forgeableComponents = Forgeables;

        forgeableComponents.ForEach(f =>
        {
            GameObject _buttonObject = Instantiate(buttonPrefab) as GameObject;
            MenuButton _button = _buttonObject.GetComponent<MenuButton>();

            ItemPrice _price = f.gameObject.GetComponent<ItemPrice>();

            _button.Initialize(f.Name, _price.CreditValue.ToString());
            _button.OnActionMain += ProcesForgeRequest;


            m_Menu.AddButton(_buttonObject);

            buttonToForgeDictionary.Add(_button, f);
        });
    }

    void ProcesForgeRequest(MenuButton selectedButton)
    {
        if (!buttonToForgeDictionary.ContainsKey(selectedButton))
            return;

        ForgeableObject forgeable = buttonToForgeDictionary[selectedButton];

        ItemPrice price = forgeable.gameObject.GetComponent<ItemPrice>();
        if (!activatingPlayer.CreditArithmetic(-price.CreditValue))
        {
            return;
        }

        GameObject g = Instantiate(forgeable.gameObject) as GameObject;
        Forge(activatingPlayer, g.GetComponent<ForgeableObject>(), activatingPlayer.GetComponent<Team>());

        DeflateMenu();
    }

    protected override bool CanInflateMenu()
    {
        return base.CanInflateMenu() && CanRemoveActive();
    }

    #endregion

    

    public void Forge(PlayerController forger, ForgeableObject forgeObj)
    {
        Forge(forger, forgeObj, null);
    }
    public void Forge(PlayerController forger, ForgeableObject forgeObj, Team _team)
    {
        StartCoroutine(AttemptForgeRoutine(forger, forgeObj, _team));
    }
    IEnumerator AttemptForgeRoutine(PlayerController player, ForgeableObject forgeObj, Team team)
    {
        forgingPlayer = player;
        processingForge = forgeObj;
        IsInteractable = false;

        forgeObj.gameObject.SetActive(false);
        forgeObj.transform.position = m_Transform.position;

        timerUI = ObjectPoolerManager.Instance.DialUIPooler.GetPooledObject();
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

        processingForge = null;
        timerUI = null;

        ProcessForge(forgeObj, team);

        IsInteractable = true;
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
        Debug.Log("Cancelling Forge Attempt");
        StopAllCoroutines();

        if (processingForge != null)
        {
            ItemPrice forgePrice = processingForge.GetComponent<ItemPrice>();

            if (forgePrice != null && forgingPlayer != null)
            {
                forgingPlayer.CreditArithmetic(forgePrice.CreditValue);
            }


            Destroy(processingForge.gameObject);
            processingForge = null;
        }

        if (timerUI != null)
        {
            timerUI.SetActive(false);
            timerUI = null;
        }

        IsInteractable = true;
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

    protected override void OnValidate()
    {
        base.OnValidate();

        if (m_Stats != null)
        {
            m_Stats.Validate();
        }
    }
}
