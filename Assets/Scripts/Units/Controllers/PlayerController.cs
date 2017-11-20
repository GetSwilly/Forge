using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : UnitController
{
    static readonly float _CollectSpeed = 600f;
    static readonly float _DropUtilityDelay = 0.5f;


    [Header("Levelling")]

    [Tooltip("Current Character Level")]
    int currentLevel = 0;
    
    int currentLevelPoints = 0;


    [Tooltip("Current Character Exp")]
    int currentExp = 0;


    /*
     ########################################################
     */
    [Header("Interaction")]

    [Tooltip("Minimum distance necessary to interact with object")]
    [SerializeField]
    float interactDistance = 4f;

    [Tooltip("Should collect collectibles?")]
    [SerializeField]
    bool shouldCollect = false;

    [Tooltip("Minimum distance necessary to collect a collectible")]
    [SerializeField]
    float collectRange = 4f;

    /*
     * ########################################################
     */
    [Header("Utility")]

    [SerializeField]
    Transform handheldHolder;

    [Tooltip("Maximum throwing power for UtilityItem")]
    [SerializeField]
    float throwPower;

    [Tooltip("Time to achieve full throw power")]
    [SerializeField]
    [Range(0.1f, 4f)]
    float throwTime;

    [Tooltip("Throw direction")]
    [SerializeField]
    Vector3 throwVector = new Vector3(0f, 1f, 0f);

    float currentThrowTime = 0f;

    [Tooltip("Current Utility Item")]
    [SerializeField]
    UtilityItem m_UtilityItem;

    [Tooltip("Number of UtilityItems left in inventory")]
    [SerializeField]
    int utilityItemCount = 0;
    bool isUsingUtility = false;


    /*
     * ########################################################
     */
    [Header("Items")]


    [Tooltip("Current HandheldItem")]
    [SerializeField]
    protected HandheldItem m_HandheldItem;

    [Tooltip("Native Ability. Can not be dropped")]
    [SerializeField]
    protected Ability nativeAbility;

    [Tooltip("Auxiliary Ability. Can be dropped")]
    [SerializeField]
    protected Ability auxiliaryAbility;


    InteractableObject currentInteractable;
    Coroutine handheldPickupRoutine = null;
    Coroutine abilityPickupRoutine = null;


    public event Delegates.ValueAlertEvent OnLevelChange;
    public event Delegates.ValueAlertEvent OnLevelPointsChange;

    public event Delegates.ValueAlertEvent OnHandheldChange;
    public event Delegates.AbilityChangeEvent OnNativeAbilityChange;
    public event Delegates.AbilityChangeEvent OnAuxiliaryAbilityChange;
    public event Delegates.ValueChangeEvent OnExpChange;




    public override void Start()
    {
        base.Start();

        if (GameManager.Instance != null)
        {
            //m_Health.OnDamaged += GameManager.Instance.PlayerDamaged;
            //m_Health.OnKilled += GameManager.Instance.PlayerKilled;
        }

        //m_Handler.ShowUI(true);
        //m_Handler.UpdateUI(Attribute.Experience, CurrentExperienceLevelProgress, false);


        if (NativeAbility != null)
        {
            GameObject obj = Instantiate(NativeAbility.gameObject) as GameObject;
            obj.transform.position = m_Transform.position;

            NativeAbility = obj.GetComponent<Ability>();
            PickupNativeAbility();
        }

        if (AuxiliaryAbility != null)
        {
            GameObject obj = Instantiate(AuxiliaryAbility.gameObject) as GameObject;
            obj.transform.position = m_Transform.position;

            AuxiliaryAbility = obj.GetComponent<Ability>();
            Pickup(AuxiliaryAbility);
        }

        if (HandheldItem != null)
        {
            GameObject obj = Instantiate(HandheldItem.gameObject) as GameObject;
            obj.transform.position = m_Transform.position;

            HandheldItem = obj.GetComponent<HandheldItem>();
            Pickup(HandheldItem);
        }

        if (UtilityItem != null)
        {
            //GameObject obj = Instantiate(UtilityItem.gameObject) as GameObject;
            //obj.transform.position = m_Transform.position;

            //UtilityItem = obj.GetComponent<UtilityItem>();
            //Pickup(UtilityItem);
        }
    }
    void OnEnable()
    {
        if (ShowDebug)
        {
            Debug.Log(m_Handler.ToString());
        }
    }
    public override void OnDisable()
    {
        base.OnDisable();

        RemoveInteractable(currentInteractable);
    }





    #region Interactable Stuff

    public void Interact()
    {
        if (currentInteractable == null)
            return;


        if (!currentInteractable.IsUsable)
        {
            return;
        }

        Vector3 toVector = currentInteractable.transform.position - m_Transform.position;
        if (toVector.magnitude <= InteractDistance) // && (currentInteractable.IsUsableOutsideFOV || CanSee(currentInteractable.transform)))
        {
            currentInteractable.Interact(this);
        }

        RemoveInteractable(currentInteractable);
    }



    void AddInteractable(GameObject tempObj)
    {
        if (tempObj == null)
            return;


        InteractableObject iObj = tempObj.GetComponent<InteractableObject>();
        AddInteractable(iObj);
    }
    void AddInteractable(InteractableObject tempInteractable)
    {
        if (tempInteractable == null || !tempInteractable.IsUsable || tempInteractable == currentInteractable)
            return;



        if (currentInteractable == null || !currentInteractable.IsUsable || (!currentInteractable.IsUsableOutsideFOV && !CanSee(currentInteractable.transform) && CanSee(tempInteractable.transform))) //Vector3.Angle(tempInteractable.transform.position - myInteractable.transform.position, myTransform.forward) > FOV/2f))
        {
            RemoveInteractable(currentInteractable);
            currentInteractable = tempInteractable;
        }
        else if (Vector3.Distance(m_Transform.position, currentInteractable.transform.position) > Vector3.Distance(m_Transform.position, tempInteractable.transform.position))
        {
            RemoveInteractable(currentInteractable);
            currentInteractable = tempInteractable;
        }

        currentInteractable.InflateUI();
    }
    void RemoveInteractable(GameObject tempObj)
    {
        if (tempObj == null)
            return;


        InteractableObject iObj = tempObj.GetComponent<InteractableObject>();
        RemoveInteractable(iObj);
    }
    void RemoveInteractable(InteractableObject tempInteractable)
    {
        if (currentInteractable == null || tempInteractable == null)
            return;

        if (tempInteractable == currentInteractable)
        {
            currentInteractable.DeflateUI();
            currentInteractable = null;
        }
    }

    #endregion



    #region Utility Item Stuff

    public void ActivateUtilityItem()
    {
        if (UtilityItem.ShouldBeThrown)
        {
            currentThrowTime += Time.deltaTime;
            isUsingUtility = true;
        }
        else
        {
            UseUtilityItem();
            isUsingUtility = false;
        }
    }
    public void DeactivateUtilityItem()
    {
        if (UtilityItem.ShouldBeThrown)
        {
            UseUtilityItem();
        }

        isUsingUtility = false;
    }

    void UseUtilityItem()
    {
        float percentage = Mathf.Clamp01(currentThrowTime / throwTime);

        GameObject _obj = (GameObject)Instantiate(m_UtilityItem.gameObject, HandheldHolderPosition, m_Transform.rotation);
        UtilityItem _uScript = _obj.GetComponent<UtilityItem>();


        if (m_UtilityItem.ShouldBeThrown)
        {
            Vector3 launchVector = m_Transform.TransformDirection(throwVector).normalized * ThrowPower * percentage;


            Rigidbody _rigidbody = _obj.GetComponent<Rigidbody>();

            _obj.SetActive(true);
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(launchVector);
        }


        _uScript.Activate(m_Transform, CurrentStats);


        currentThrowTime = 0f;
        utilityItemCount--;
    }
    public void CancelUtilityThrow()
    {
        currentThrowTime = 0f;
    }

    IEnumerator DropUtilityItems(GameObject dropPrefab, int numToDrop)
    {
        GameObject genObj = GameObject.Find("Generated Objects");

        while (numToDrop > 0)
        {

            GameObject _dropObj = (GameObject)Instantiate(dropPrefab, Vector3.zero, Quaternion.identity);
            _dropObj.transform.parent = genObj == null ? null : genObj.transform;

            _dropObj.transform.position = HandheldHolderPosition;
            _dropObj.transform.localRotation = Quaternion.identity;
            _dropObj.SetActive(true);

            Rigidbody _rigid = _dropObj.GetComponent<Rigidbody>();
            if (_rigid != null)
            {
                _rigid.velocity = Vector3.zero;
                _rigid.AddForce(_dropObj.transform.forward * _PickupDropForce, ForceMode.Impulse);
            }

            numToDrop--;

            yield return new WaitForSeconds(_DropUtilityDelay);
        }

        GameObject.Destroy(dropPrefab);
    }

    #endregion


    #region Offensive Stuff

    public void ActivateNativeAbility()
    {
        if (nativeAbility == null || !nativeAbility.CanUseAbility())
            return;

        nativeAbility.ActivateAbility();

    }
    public void DeactivateNativeAbility()
    {
        if (nativeAbility == null)
            return;

        nativeAbility.DeactivateAbility();
    }


    public void ActivateAuxiliaryAbility()
    {
        if (auxiliaryAbility == null || !auxiliaryAbility.CanUseAbility())
            return;

        auxiliaryAbility.ActivateAbility();
    }
    public void DeactivateAuxiliaryAbility()
    {
        if (auxiliaryAbility == null)
            return;

        auxiliaryAbility.DeactivateAbility();

    }


    public void ActivateHandheldPrimary()
    {
        if (m_HandheldItem == null || !m_HandheldItem.CanActivatePrimary())
            return;

        m_HandheldItem.ActivatePrimary();
        //CameraShake.Instance.Shake(myWeapon.ShakeAmountPrimary, myWeapon.ShakeTime);

    }
    public void DeactivateHandheldPrimary()
    {
        if (m_HandheldItem == null)
            return;

        m_HandheldItem.DeactivatePrimary();
    }
    public void ActivateHandheldSecondary()
    {
        if (m_HandheldItem == null || !m_HandheldItem.CanActivateSecondary())
            return;

        m_HandheldItem.ActivateSecondary();
        //CameraShake.Instance.Shake(myWeapon.ShakeAmountSecondary, myWeapon.ShakeTime);

    }
    public void DeactivateHandheldSecondary()
    {
        if (m_HandheldItem == null)
            return;

        m_HandheldItem.DeactivateSecondary();
    }


    public void ActivateHandheldTertiary()
    {
        if (m_HandheldItem == null || !m_HandheldItem.CanActivateTertiary())
            return;


        m_HandheldItem.ActivateTertiary();
    }
    public void DeactivateHandheldUtility()
    {
        if (m_HandheldItem == null)
            return;

        m_HandheldItem.DeactivateTertiary();
    }


    private void PickupNativeAbility()
    {
        NativeAbility = NativeAbility;
        NativeAbility.transform.parent = m_Transform;
        NativeAbility.transform.localPosition = rootPosition;
        abilityPickupRoutine = StartCoroutine(PickupObject(NativeAbility.transform, rootPosition, Quaternion.identity));
        NativeAbility.Initialize(m_Transform);

        ItemPickup _pickup = NativeAbility.GetComponent<ItemPickup>();
        _pickup.enabled = false;

        Rigidbody _rigidbody = NativeAbility.GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;

        Collider[] _colliders = NativeAbility.GetComponentsInChildren<Collider>();
        for (int i = 0; i < _colliders.Length; i++) { _colliders[i].enabled = false; }

        NativeAbility.OnAbilityChanged += NativeAbilityChanged;
    }
    public void Pickup(Ability newAbility)
    {
        if (newAbility == null)
            return;

        DropAbility();

        auxiliaryAbility = newAbility;
        auxiliaryAbility.transform.parent = m_Transform;
        abilityPickupRoutine = StartCoroutine(PickupObject(AuxiliaryAbility.transform, rootPosition, Quaternion.identity));
        auxiliaryAbility.Initialize(m_Transform);

        ItemPickup _pickup = AuxiliaryAbility.GetComponent<ItemPickup>();
        _pickup.enabled = false;

        Rigidbody _rigidbody = AuxiliaryAbility.GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;

        Collider[] _colliders = AuxiliaryAbility.GetComponentsInChildren<Collider>();
        for (int i = 0; i < _colliders.Length; i++) { _colliders[i].enabled = false; }

        AuxiliaryAbility.OnAbilityChanged += AuxiliaryAbilityChanged;
    }

    public virtual void DropAbility(Ability _ability)
    {
        if (auxiliaryAbility == _ability)
        {
            DropAbility();
        }
    }
    protected virtual void DropAbility()
    {
        if (auxiliaryAbility != null)
        {

            auxiliaryAbility.Terminate();


            GameObject g = GameManager.Instance.generatedObjectHolder;
            auxiliaryAbility.transform.parent = null;

            if (g != null)
            {
                auxiliaryAbility.transform.parent = g.transform;
            }


            auxiliaryAbility.transform.localRotation = Quaternion.identity;



            ItemPickup _pickup = auxiliaryAbility.GetComponent<ItemPickup>();

            if (_pickup != null)
                _pickup.enabled = true;

            Rigidbody _rigidbody = auxiliaryAbility.GetComponent<Rigidbody>();
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(auxiliaryAbility.transform.forward * _PickupDropForce, ForceMode.Impulse);


            Collider[] _colliders = AuxiliaryAbility.GetComponentsInChildren<Collider>();
            for (int i = 0; i < _colliders.Length; i++) { _colliders[i].enabled = true; }


            if (abilityPickupRoutine != null)
                StopCoroutine(abilityPickupRoutine);
        }
    }



    public void Pickup(HandheldItem newHandheld)
    {

        if (newHandheld == null)
            return;

        DropHandheld();

        m_HandheldItem = newHandheld;
        m_HandheldItem.transform.parent = handheldHolder;
        m_HandheldItem.transform.localPosition = Vector3.zero;
        m_HandheldItem.transform.localRotation = Quaternion.identity;
        // handheldPickupRoutine = StartCoroutine(PickupObject(m_HandheldItem.transform, handheldHolder.position, Quaternion.identity));

        ItemPickup _pickup = m_HandheldItem.GetComponent<ItemPickup>();
        _pickup.enabled = false;

        Rigidbody _rigidbody = m_HandheldItem.GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;

        Collider[] _colliders = m_HandheldItem.GetComponentsInChildren<Collider>();
        for (int i = 0; i < _colliders.Length; i++) { _colliders[i].enabled = false; }


        //m_HandheldItem.OnActivatePrimary += HandheldActivationPrimary;
        //m_HandheldItem.OnActivateSecondary += HandheldActivationSecondary;
        //m_HandheldItem.OnActivateUtility += HandheldActivationUtility;
        m_HandheldItem.OnWeaponChanged += HandheldChanged;
        m_HandheldItem.OnWeaponCasualty += CasualtyAchieved;

        if (m_HandheldItem is Weapon)
        {
            Weapon _weapon = (Weapon)m_HandheldItem;

            //_weapon.BonusAttackPower = GetStatValue(StatType.Damage);
            //_weapon.BonusCriticalHitChance = GetStatValue(StatType.Luck);
            //_weapon.BonusCriticalHitMultiplier = GetStatValue(StatType.CriticalDamage);
        }

        m_HandheldItem.Initialize(m_Transform, m_Team);
        //m_HandheldItem.SetVolume(currentLevel);
    }

    public virtual void DropHandheld(HandheldItem _item)
    {
        if (m_HandheldItem == _item)
        {
            DropHandheld();
        }
    }
    protected virtual void DropHandheld()
    {
        if (m_HandheldItem == null)
            return;

        m_HandheldItem.Terminate();


        GameObject genObj = GameObject.Find("Generated Objects");
        Transform newParent = (genObj == null) ? null : genObj.transform;

        m_HandheldItem.transform.parent = newParent;
        m_HandheldItem.transform.localRotation = Quaternion.identity;

        ItemPickup _pickup = m_HandheldItem.GetComponent<ItemPickup>();

        if (_pickup != null)
            _pickup.enabled = true;

        Rigidbody _rigidbody = m_HandheldItem.GetComponent<Rigidbody>();
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(auxiliaryAbility.transform.forward * _PickupDropForce, ForceMode.Impulse);

        Collider[] _colliders = m_HandheldItem.GetComponentsInChildren<Collider>();
        for (int i = 0; i < _colliders.Length; i++) { _colliders[i].enabled = true; }

        m_HandheldItem = null;


        if (handheldPickupRoutine != null)
            StopCoroutine(handheldPickupRoutine);

        handheldPickupRoutine = null;

    }


    //void HandheldActivationPrimary()
    //{
    //    if (m_HandheldItem == null)
    //        return;

    //    CameraShake.Instance.Shake(m_HandheldItem.ShakeAmountPrimary, m_HandheldItem.ShakeTime);
    //}
    //void HandheldActivationSecondary()
    //{
    //    if (m_HandheldItem == null)
    //        return;

    //    CameraShake.Instance.Shake(m_HandheldItem.ShakeAmountSecondary, m_HandheldItem.ShakeTime);
    //}
    //void HandheldActivationUtility()
    //{
    //    if (m_HandheldItem == null)
    //        return;

    //    CameraShake.Instance.Shake(m_HandheldItem.ShakeAmountTertiary, m_HandheldItem.ShakeTime);
    //}

    #endregion


    #region Stat Stuff

    //protected override void UpdateStatEffects(StatType _type)
    //   {
    //	base.UpdateStatEffects(_type);

    //	Stat _stat = GetStat(_type);

    //       if (_stat == null || m_HandheldItem == null)
    //           return;

    //       m_HandheldItem.UpdateStat(new CustomTuple2<StatType, float>(_stat.Type, _stat.CurrentValue));

    //       //if(_stat == null || m_HandheldItem == null || !(m_HandheldItem is Weapon))
    //       //	return;


    //       //      Weapon _weapon = (Weapon)m_HandheldItem;


    //       //switch(_type)
    //       //      {
    //       //          case StatType.Dexterity:
    //       //              _weapon.BonusAttackRate = _stat.CurrentValue;

    //       //              break;
    //       //          case StatType.Damage:
    //       //              _weapon.BonusAttackPower = _stat.CurrentValue;

    //       //	    break;
    //       //    case StatType.CriticalDamage:
    //       //              _weapon.BonusCriticalHitMultiplier = _stat.CurrentValue;

    //       //	    break;
    //       //    case StatType.Luck:
    //       //              _weapon.BonusCriticalHitChance = _stat.CurrentValue;

    //       //	    break;
    //       //    default:
    //       //	    break;
    //       //}
    //   }

    #endregion


    #region Experience Stuff

    public void ModifyExperiencePoints(int experienceDelta)
    {
        if (experienceDelta == 0)
            return;

        CurrentExperience += experienceDelta;

        if (CurrentExperience > GetExpRequiredForLevel(CurrentLevel))
        {
            CurrentLevel++;

            ModifyLevelPoints(1);

            if (OnLevelChange != null)
            {
                OnLevelChange(CurrentLevel);
            }
        }

        if (UIManager.Instance != null)
        {
            if (experienceDelta > 0)
            {
                UIManager.Instance.CreateDynamicInfoScript(transform.position, experienceDelta, Colors._ExperienceColor);
            }
            else
            {
                UIManager.Instance.CreateDynamicInfoScript(transform.position, experienceDelta, Colors._ExperienceColor);
            }
        }

        if (OnExpChange != null)
        {
            OnExpChange(CurrentExperienceLevelProgress, experienceDelta);
        }
    }

    public bool CanModifyExp(int delta)
    {
        int temp = CurrentExperience + delta;
       
        return temp >= GetExpRequiredForLevel(CurrentLevel - 1); // ? false : true;
    }

    public void ResetExp()
    {
        int requiredExp = GetExpRequiredForLevel(CurrentLevel);

        ModifyExperiencePoints(requiredExp - CurrentExperience);
    }

    public int GetExpRequiredForLevel(int lvl)
    {
        if (lvl <= 0)
            return 0;

        return 100 + (int)(25 * Mathf.Pow(lvl, 1.8f));
    }


    #endregion


    public bool CanModifyLevelPoints(int delta)
    {
        return LevelPoints + delta >= 0;
    }
    public void ModifyLevelPoints(int delta)
    {
        if (LevelPoints + delta < 0)
        {
            return;
        }


        LevelPoints += delta;

        if(OnLevelPointsChange != null)
        {
            OnLevelPointsChange(LevelPoints);
        }
    }



    protected void PlayEffect(DisplayEffect _effect)
    {
        _effect.Play();
    }



    public override void UpdateUI()
    {
        base.UpdateUI();

        HandheldChanged(HandheldItem == null ? 0f : HandheldItem.GetPercentage());

        if (NativeAbility == null)
        {
            NativeAbilityChanged("", 0f);
        }
        else
        {
            NativeAbilityChanged(NativeAbility.Name, NativeAbility.GetChargePercentage());
        }

        if (AuxiliaryAbility == null)
        {
            AuxiliaryAbilityChanged("", 0f);
        }
        else
        {
            AuxiliaryAbilityChanged(AuxiliaryAbility.Name, AuxiliaryAbility.GetChargePercentage());
        }

        if (OnExpChange != null)
        {
            OnExpChange(CurrentExperienceLevelProgress, 0);
        }

        if (OnLevelChange != null)
        {
            OnLevelChange(CurrentLevel);
        }
    }

    protected void HandheldChanged(float percentage)
    {
        if (OnHandheldChange != null)
        {
            OnHandheldChange(percentage);
        }
    }

    private void NativeAbilityChanged(string name, float percentage)
    {
        if (OnNativeAbilityChange != null)
        {
            OnNativeAbilityChange(name, percentage);
        }
    }
    private void AuxiliaryAbilityChanged(string name, float percentage)
    {
        if (OnAuxiliaryAbilityChange != null)
        {
            OnAuxiliaryAbilityChange(name, percentage);
        }
    }


    //public override void DamageAchieved(Health _casualtyHealth)
    //{
    //    IIdentifier identifier = _casualtyHealth.GetComponent<IIdentifier>();
    //    UIEventArgs args = new UIEventArgs(UIManager.Component.Enemy, (identifier != null ? identifier.Name : ""), _casualtyHealth.HealthPercentage, false);

    //    OnUIAttributeChanged(args);

    //    if (_casualtyHealth.IsAlive)
    //    {
    //        if (nativeAbility != null)
    //            nativeAbility.DamageAchieved(_casualtyHealth.LastHealthChange);

    //        if (auxiliaryAbility != null)
    //            auxiliaryAbility.DamageAchieved(_casualtyHealth.LastHealthChange);

    //        //if(SoundManager.Instance != null)
    //        //	SoundManager.Instance.PlaySound(damageAchievedEffect);

    //        PlayEffect(damageAchievedEffect);
    //    }
    //    else
    //    {
    //        if (nativeAbility != null)
    //            nativeAbility.KillAchieved();

    //        if (auxiliaryAbility != null)
    //            auxiliaryAbility.KillAchieved();

    //        //if(SoundManager.Instance != null)
    //        //	SoundManager.Instance.PlaySound(killAchievedEffect);

    //        PlayEffect(killAchievedEffect);
    //    }
    //}



    #region Accessors

    public List<Stat> CurrentStats
    {
        get
        {
            List<Stat> _currentstats = new List<Stat>();

            StatType[] _types = Enum.GetValues(typeof(StatType)) as StatType[];

            for (int i = 0; i < _types.Length; i++)
            {
                Stat _stat = GetStat(_types[i]);

                if (_stat == null)
                    continue;

                _currentstats.Add(_stat);
            }


            return _currentstats;
        }
    }

    public bool ShouldCollect
    {
        get { return shouldCollect; }
        set { shouldCollect = value; }
    }
    public float CollectRange
    {
        get { return collectRange; }
        private set { collectRange = Mathf.Clamp(value, 0f, value); }
    }


    public int CurrentLevel
    {
        get { return currentLevel; }
        private set { currentLevel = Mathf.Clamp(value, 0, value); }
    }
    public int LevelPoints
    {
        get { return currentLevelPoints; }
        private set { currentLevelPoints = Mathf.Clamp(value, 0, value); }
    }
    public int CurrentExperience
    {
        get { return currentExp; }
        private set { currentExp = Mathf.Clamp(value, 0, value); }
    }
    public float CurrentExperienceLevelProgress
    {
        get
        {
            int totalDiff = GetExpRequiredForLevel(CurrentLevel) - GetExpRequiredForLevel(CurrentLevel - 1);
            int curDiff = CurrentExperience - GetExpRequiredForLevel(CurrentLevel - 1);


            //Debug.Log(string.Format("Current Experience: {0}. Exp Required for current"))

            return Mathf.Clamp01(curDiff / (float)totalDiff);
        }
    }



    public float InteractDistance
    {
        get { return interactDistance; }
        private set { interactDistance = Mathf.Clamp(value, 0f, value); }
    }
    public float ThrowPower
    {
        get { return throwPower; }
        private set { throwPower = Mathf.Clamp(value, 0f, value); }
    }

    public bool HasNativeAbility
    {
        get { return nativeAbility != null; }
    }
    public Ability NativeAbility
    {
        get { return nativeAbility; }
        private set { nativeAbility = value; }
    }


    public bool HasAuxiliaryAbility
    {
        get { return auxiliaryAbility != null; }
    }
    public Ability AuxiliaryAbility
    {
        get { return auxiliaryAbility; }
        private set { auxiliaryAbility = value; }
    }


    public bool HasHandheld
    {
        get { return m_HandheldItem != null; }
    }
    public HandheldItem HandheldItem
    {
        get { return m_HandheldItem; }
        private set { m_HandheldItem = value; }
    }
    public Vector3 HandheldHolderPosition
    {
        get { return handheldHolder == null ? m_Transform.position : handheldHolder.position; }
    }

    public bool HasUtilityItem
    {
        get { return m_UtilityItem != null && utilityItemCount > 0; }
    }
    public UtilityItem UtilityItem
    {
        get { return m_UtilityItem; }
    }
    public bool IsUsingUtility
    {
        get { return isUsingUtility; }
    }

    #endregion

    bool IsDesired(CollectibleItem item)
    {
        switch (item.ItemType)
        {
            case CollectibleItem.Type.Credit:
            case CollectibleItem.Type.Experience:
                return true;
            case CollectibleItem.Type.Health:
                return m_Health.NeedsHealth;
        }

        return false;
    }

    protected override void SightGained(GameObject obj)
    {
        //      if (obj.transform == m_Transform)
        //          return;
        //      /*
        //IInteractable _interactable = coll.GetComponent<IInteractable>();

        //if(_interactable != null && _interactable.IsEnabled() && _interactable.IsUsableOutsideFOV()){
        //	AddInteractable(coll.gameObject);
        //}*/

        //      if (!coll.isTrigger)
        //      {
        //          AttributeHandler _handler = coll.GetComponent<AttributeHandler>();
        //          if (_handler != null)
        //          {
        //              //_handler.ShowUI(false);
        //          }
        //      }
    }

    protected override void SightMaintained(GameObject obj)
    {
        if (obj.transform == m_Transform)
            return;

        Vector3 directionVector = obj.transform.position - m_Transform.position;

        if (ShowDebug)
        {
            //Debug.DrawLine(m_Transform.position, obj.transform.position, Color.yellow);
        }

        CollectibleItem _collectible = obj.GetComponent<CollectibleItem>();

        if (shouldCollect && _collectible != null && directionVector.magnitude <= CollectRange && IsDesired(_collectible))
        {
            //obj.transform.position = Vector3.MoveTowards(obj.transform.position, m_Transform.position, EXP_COLLECT_SPEED * Time.deltaTime);
            Rigidbody _rigidbody = obj.GetComponent<Rigidbody>();

            if (directionVector.magnitude <= CollectRange && _rigidbody != null)
            {
                Vector3 forceVector = -directionVector.normalized * _CollectSpeed * Time.deltaTime;
                _rigidbody.AddForce(forceVector, ForceMode.Force);
                //_rigidbody.MovePosition(_rigidbody.position + forceVector);
            }
        }




        InteractableObject _interactable = obj.GetComponent<InteractableObject>();

        if (_interactable != null && _interactable.IsUsable)
        {
            if (directionVector.magnitude <= InteractDistance && (_interactable.IsUsableOutsideFOV || Vector3.Angle(directionVector, m_Transform.forward) <= FOV / 2f))
            {
                AddInteractable(obj.gameObject);
            }
            else
            {
                RemoveInteractable(obj.gameObject);
            }
        }



    }

    protected override void SightLost(GameObject obj)
    {
        if (obj == null || obj.transform == m_Transform)
            return;

        InteractableObject _interactable = obj.GetComponent<InteractableObject>();
        if (_interactable != null)
            RemoveInteractable(obj);
    }

    #region OnCollision / OnTrigger

    //  public virtual void OnTriggerEnter(Collider coll)
    //  {
    //      if (coll.transform == m_Transform || Utilities.IsInLayerMask(coll.gameObject, ignoreCollisionLayer))
    //          return;
    //      /*
    //IInteractable _interactable = coll.GetComponent<IInteractable>();

    //if(_interactable != null && _interactable.IsEnabled() && _interactable.IsUsableOutsideFOV()){
    //	AddInteractable(coll.gameObject);
    //}*/

    //      if (!coll.isTrigger)
    //      {
    //          AttributeHandler _handler = coll.GetComponent<AttributeHandler>();
    //          if (_handler != null)
    //          {
    //              //_handler.ShowUI(false);
    //          }
    //      }
    //  }

    //  public virtual void OnTriggerStay(Collider coll)
    //  {
    //      if (coll.transform == m_Transform || Utilities.IsInLayerMask(coll.gameObject, ignoreCollisionLayer))
    //          return;

    //      Vector3 toVector = coll.transform.position - m_Transform.position;

    //      if (ShowDebug)
    //      {
    //          Debug.DrawLine(m_Transform.position, coll.transform.position, Color.yellow);
    //          //Debug.DrawLine(myTransform.position, myTransform.position + (myTransform.forward * 5f), Color.cyan);
    //      }

    //      ICollectible _collectible = coll.GetComponent<ICollectible>();

    //      if (shouldCollect && _collectible != null)
    //      {
    //          Rigidbody _rigidbody = coll.GetComponent<Rigidbody>();

    //          if (toVector.magnitude <= CollectRange && _rigidbody != null)
    //          {
    //              Vector3 forceVector = -toVector * EXP_COLLECT_SPEED;
    //              _rigidbody.AddForce(forceVector);
    //          }
    //      }




    //      InteractableObject _interactable = coll.GetComponent<InteractableObject>();

    //      if (_interactable != null && _interactable.IsUsable)
    //      {
    //          if (toVector.magnitude <= InteractDistance && (_interactable.IsUsableOutsideFOV || Vector3.Angle(toVector, m_Transform.forward) <= FOV / 2f))
    //          {
    //              AddInteractable(coll.gameObject);
    //          }
    //          else
    //          {
    //              RemoveInteractable(coll.gameObject);
    //          }
    //      }



    //  }

    //  public virtual void OnTriggerExit(Collider coll)
    //  {
    //      if (coll.transform == m_Transform || Utilities.IsInLayerMask(coll.gameObject, ignoreCollisionLayer))
    //          return;

    //      InteractableObject _interactable = coll.GetComponent<InteractableObject>();

    //      if (_interactable != null)
    //          RemoveInteractable(coll.gameObject);


    //      if (!coll.isTrigger)
    //      {
    //          AttributeHandler _handler = coll.GetComponent<AttributeHandler>();
    //          if (_handler != null)
    //          {
    //              //_handler.HideUI();
    //          }
    //      }
    //  }

    #endregion



    public bool Equals(PlayerController other)
    {
        return Name.Equals(other.Name);
    }
    public void Copy(PlayerController other)
    {
        if (other == null)
            return;


        Copy(other as UnitController);

        Ability _ability = other.AuxiliaryAbility;
        HandheldItem _handheld = other.HandheldItem;

        other.DropAbility();
        other.DropHandheld();

        Pickup(_ability);
        Pickup(_handheld);


        CurrentLevel = other.CurrentLevel;
        CurrentExperience = other.CurrentExperience;
    }




    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (showDebug && m_Transform != null)
        {

            if (currentInteractable != null && currentInteractable.gameObject.activeInHierarchy)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawLine(m_Transform.position, currentInteractable.transform.position);
            }



            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_Transform.position, m_Transform.position + (m_Transform.forward * SightRange));
        }


    }

    public override void OnValidate()
    {
        base.OnValidate();

        CurrentLevel = CurrentLevel;
        CurrentExperience = CurrentExperience;

        CollectRange = CollectRange;

        InteractDistance = InteractDistance;
        ThrowPower = ThrowPower;

    }
}

