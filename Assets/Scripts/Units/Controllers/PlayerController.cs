using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : UnitController {


	static readonly float EXP_COLLECT_SPEED = 1f;
    static readonly float DROP_UTILITY_DELAY = 0.5f;

	


    [Space(15)]
    [Header("Levelling")]
    [Space(5)]

    [Tooltip("Current Character Level")]
    [SerializeField]
    int currentLevel = 1;

    [Tooltip("Current Character Exp")]
    [SerializeField]
    int currentExp = 0;

	int numNewLevels = 0;

    [Tooltip("Attract Exp orbs from a distance?")]
    [SerializeField]
    bool shouldCollectExp = false;

    [Tooltip("Range to attract Exp orbs")]
    [SerializeField]
    float expCollectRange = 2;
   
    [Tooltip("Minimum distance necessary to interact with object")]
    [SerializeField]
    float interactDistance = 4f;



    [Space(15)]
    [Header("Utility")]
    [Space(5)]


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




    [Space(15)]
    [Header("Items")]
    [Space(5)]
    
    
    [Tooltip("Current HandheldItem")]
    [SerializeField]
    protected HandheldItem m_HandheldItem;

    [Tooltip("Native Ability. Can not be dropped")]
    [SerializeField]
    protected Ability nativeAbility;

    [Tooltip("Auxiliary Ability. Can be dropped")]
    [SerializeField]
    protected Ability auxiliaryAbility;




    [Space(15)]
    [Header("Effects")]
    [Space(5)]

    [Tooltip("Effect to be played upon damaging an enemy")]
    [SerializeField]
    DisplayEffect damageAchievedEffect;

    [Tooltip("Effect to be played upon killing an enemy")]
    [SerializeField]
    DisplayEffect killAchievedEffect;


    [Tooltip("Effect to be played upon gaining experience")]
    [SerializeField]
    DisplayEffect experienceGainedEffect;

    [Tooltip("Effect to be played upon losing experience")]
    [SerializeField]
    DisplayEffect experienceLostEffect;



    InteractableObject m_Interactable;

    Coroutine handheldPickupRoutine = null;
    Coroutine abilityPickupRoutine = null;


    public delegate void AlertEvent();
    public event AlertEvent OnExpChange;


    [SerializeField]
    string characterName = "";




    public override void Start()
    {
		base.Start();

		if(GameManager.Instance != null)
        {
			m_Health.OnDamaged += GameManager.Instance.PlayerDamaged;
			m_Health.OnKilled += GameManager.Instance.PlayerKilled;
		}

		
		OnExpChange += UpdateExperienceProgressBar;

        m_Handler.ShowUI(true);
        m_Handler.UpdateUI(Attribute.Experience, CurrentExperienceLevelProgress, false);


        if (NativeAbility != null)
        {
            GameObject obj = Instantiate(NativeAbility.gameObject) as GameObject;
            obj.transform.position = m_Transform.position;

            NativeAbility = obj.GetComponent<Ability>();
            PickupNativeAbility();
        }

        if(AuxiliaryAbility != null)
        {
            GameObject obj = Instantiate(AuxiliaryAbility.gameObject) as GameObject;
            obj.transform.position = m_Transform.position;

            AuxiliaryAbility = obj.GetComponent<Ability>();
            Pickup(AuxiliaryAbility);
        }

        if(HandheldItem != null)
        {
            GameObject obj = Instantiate(HandheldItem.gameObject) as GameObject;
            obj.transform.position = m_Transform.position;

            HandheldItem = obj.GetComponent<HandheldItem>();
            Pickup(HandheldItem);
        }

        if(UtilityItem != null)
        {
            //GameObject obj = Instantiate(UtilityItem.gameObject) as GameObject;
            //obj.transform.position = m_Transform.position;

        //UtilityItem = obj.GetComponent<UtilityItem>();
            //Pickup(UtilityItem);
        }

		//HandheldItem startingHandheld = GetComponentInChildren<HandheldItem>();
		//Pickup(startingHandheld);
        
		//if(nativeAbility != null)
  //      {
  //          nativeAbility.transform.SetParent(m_Transform, true);
		//	StartCoroutine(PickupObject(nativeAbility.transform, Vector3.zero, Quaternion.identity));
			
			
		//	ItemPickup _pickup = nativeAbility.GetComponent<ItemPickup>();
		//	_pickup.enabled = false;
			
		//	Rigidbody _rigidbody = nativeAbility.GetComponent<Rigidbody>();
  //          //_rigidbody.useGravity = false;
		//	_rigidbody.isKinematic = true;
			
		//	Collider _collider = nativeAbility.GetComponent<Collider>();
		//	_collider.enabled = false;


  //          nativeAbility.Initialize(m_Transform);
		//}


		//Pickup(auxiliaryAbility);



		m_Health.ReverseTextMovement();
	}
    void OnEnable()
    {
       m_Handler.ShowUI(true);

        if (ShowDebug)
        {
            Debug.Log(m_Handler.ToString());
        }
    }
    public override void OnDisable()
    {
        base.OnDisable();

        RemoveInteractable(m_Interactable);
    }


   

    #region Interactable Stuff

    public void Interact(int interactionType)
    {

        if (m_Interactable == null)
            return;


        if (!m_Interactable.IsUsable)
            return;

        Vector3 toVector = m_Interactable.transform.position - m_Transform.position;
        if (toVector.magnitude <= InteractDistance && (m_Interactable.IsUsableOutsideFOV || CanSee(m_Interactable.transform)))//Vector3.Angle(toVector, myTransform.forward) <= FOV / 2f))
        {
            switch (interactionType)
            {
                case 1:
                    m_Interactable.Use(this);
                    break;
                case 2:
                    m_Interactable.Give(this);
                    break;
            }
        }

        RemoveInteractable(m_Interactable);
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
        if (tempInteractable == null || !tempInteractable.IsUsable || tempInteractable == m_Interactable)
            return;



        if (m_Interactable == null || !m_Interactable.IsUsable || (!m_Interactable.IsUsableOutsideFOV && !CanSee(m_Interactable.transform) && CanSee(tempInteractable.transform))) //Vector3.Angle(tempInteractable.transform.position - myInteractable.transform.position, myTransform.forward) > FOV/2f))
        {
            RemoveInteractable(m_Interactable);
            m_Interactable = tempInteractable;
        }
        else if(Vector3.Distance(m_Transform.position, m_Interactable.transform.position) > Vector3.Distance(m_Transform.position, tempInteractable.transform.position))
        {
            RemoveInteractable(m_Interactable);
            m_Interactable = tempInteractable;
        }

        m_Interactable.InflateUI();
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
        if (m_Interactable == null || tempInteractable == null)
            return;

        if(tempInteractable == m_Interactable)
        {
            m_Interactable.DeflateUI();
            m_Interactable = null;
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
            if(_rigid != null)
            {
                _rigid.velocity = Vector3.zero;
                _rigid.AddForce(_dropObj.transform.forward * DROP_FORCE, ForceMode.Impulse);
            }

            numToDrop--;

            yield return new WaitForSeconds(DROP_UTILITY_DELAY);
        }

        GameObject.Destroy(dropPrefab);
    }

    #endregion





    public override void NoiseHeard(AudioClip noise, Transform noiseOwner, Vector3 noisePosition, float noiseVolume)
    {
        if (noiseVolume < HearingThreshold)
            return;
    }




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
        abilityPickupRoutine = StartCoroutine(PickupObject(NativeAbility.transform, Vector3.zero, Quaternion.identity));
        NativeAbility.Initialize(m_Transform);

        ItemPickup _pickup = NativeAbility.GetComponent<ItemPickup>();
        _pickup.enabled = false;

        Rigidbody _rigidbody = NativeAbility.GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;

        Collider _collider = NativeAbility.GetComponent<Collider>();
        _collider.enabled = false;
    }
	public void Pickup(Ability newAbility)
    {
		if(newAbility == null)
			return;

		DropAbility();

		auxiliaryAbility = newAbility;
		auxiliaryAbility.transform.parent = m_Transform;
		abilityPickupRoutine = StartCoroutine(PickupObject(auxiliaryAbility.transform, Vector3.zero, Quaternion.identity));
        auxiliaryAbility.Initialize(m_Transform);

		ItemPickup _pickup = auxiliaryAbility.GetComponent<ItemPickup>();
		_pickup.enabled = false;
		
		Rigidbody _rigidbody = auxiliaryAbility.GetComponent<Rigidbody>();
		_rigidbody.isKinematic = true;
		
		Collider _collider = auxiliaryAbility.GetComponent<Collider>();
		_collider.enabled = false;
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
		if(auxiliaryAbility != null)
        {

            auxiliaryAbility.Terminate();


			GameObject g = GameObject.Find("Generated Objects");
			auxiliaryAbility.transform.parent = null;

			if(g != null)
            {
				auxiliaryAbility.transform.parent = g.transform;
			}


            auxiliaryAbility.transform.localRotation = Quaternion.identity;


			
			ItemPickup _pickup = auxiliaryAbility.GetComponent<ItemPickup>();
			
			if(_pickup != null)
				_pickup.enabled = true;

			Rigidbody _rigidbody = auxiliaryAbility.GetComponent<Rigidbody>();
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.isKinematic = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(auxiliaryAbility.transform.forward * DROP_FORCE, ForceMode.Impulse);
           

            Collider _collider = auxiliaryAbility.GetComponent<Collider>();
			_collider.enabled = true;


			if(abilityPickupRoutine != null)
				StopCoroutine(abilityPickupRoutine);
		}
	}



	public void Pickup(HandheldItem newHandheld)
    {
		
		if(newHandheld == null)
			return;
		
		DropHandheld();

        m_HandheldItem = newHandheld;
        m_HandheldItem.transform.parent = m_Transform;
		handheldPickupRoutine = StartCoroutine(PickupObject(m_HandheldItem.transform, handheldHolder.localPosition, Quaternion.identity));

		ItemPickup _pickup = m_HandheldItem.GetComponent<ItemPickup>();
		_pickup.enabled = false;

		Rigidbody _rigidbody = m_HandheldItem.GetComponent<Rigidbody>();
		_rigidbody.isKinematic = true;

		Collider _collider = m_HandheldItem.GetComponent<Collider>();
		_collider.enabled = false;

       


        m_HandheldItem.OnActivatePrimary += HandheldActivationPrimary;
        m_HandheldItem.OnActivateSecondary += HandheldActivationSecondary;
        m_HandheldItem.OnActivateUtility += HandheldActivationUtility;
        m_HandheldItem.OnWeaponChanged += UpdateHandheldProgressBar;
        m_HandheldItem.OnWeaponCasualty += CasualtyAchieved;

        if (m_HandheldItem is Weapon)
        {
            Weapon _weapon = (Weapon)m_HandheldItem;


            _weapon.FriendlyMask = friendlyMask;
            //_weapon.BonusAttackPower = GetStatValue(StatType.Damage);
            //_weapon.BonusCriticalHitChance = GetStatValue(StatType.Luck);
            //_weapon.BonusCriticalHitMultiplier = GetStatValue(StatType.CriticalDamage);
        }

        m_HandheldItem.Initialize(m_Transform, CurrentStats);
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
        _rigidbody.AddForce(auxiliaryAbility.transform.forward * DROP_FORCE, ForceMode.Impulse);


        Collider _collider = m_HandheldItem.GetComponent<Collider>();
        _collider.enabled = true;

        m_HandheldItem = null;


        if (handheldPickupRoutine != null)
            StopCoroutine(handheldPickupRoutine);

        handheldPickupRoutine = null;

    }


	





	void HandheldActivationPrimary()
    {
        if (m_HandheldItem == null)
            return;

		CameraShake.Instance.Shake(m_HandheldItem.ShakeAmountPrimary, m_HandheldItem.ShakeTime);
	}
	void HandheldActivationSecondary()
    {
        if (m_HandheldItem == null)
            return;

        CameraShake.Instance.Shake(m_HandheldItem.ShakeAmountSecondary, m_HandheldItem.ShakeTime);
	}
	void HandheldActivationUtility()
    {
        if (m_HandheldItem == null)
            return;

        CameraShake.Instance.Shake(m_HandheldItem.ShakeAmountTertiary, m_HandheldItem.ShakeTime);
    }
	
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

    public void ModifyExp(int delta)
    {
		if(delta == 0)
			return;

        CurrentExperience += delta;
        

		CheckLevel();


		if(SoundManager.Instance != null)
        {
            if (delta > 0)
            {
                //SoundManager.Instance.PlaySound(expGainedSound);

                PlayEffect(experienceGainedEffect);
            }
            else
            {
                //SoundManager.Instance.PlaySound(expLostSound);
                PlayEffect(experienceLostEffect);
            }
		}

		if(OnExpChange != null)
        {
			OnExpChange();
		}
	}

	public bool CanModifyExp(int delta)
    {
		int temp = CurrentExperience + delta;

		return temp >= GetExpRequiredForLevel(CurrentLevel); // ? false : true;
	}


	public int GetLevelPoints(int delta)
    {

		if(delta <= 0)
			return 0;

		int temp = numNewLevels - delta;


		if(temp < 0)
			return 0;


		numNewLevels = temp;

		return delta;
	}


	

	public void ResetExp()
    {
		CurrentExperience = GetExpRequiredForLevel(CurrentLevel);
		
		if(OnExpChange != null)
        {
			OnExpChange();
		}
	}

	
	void CheckLevel()
    {
		int currentExpRequired = GetExpRequiredForLevel(CurrentLevel);
		int nextExpRequired = GetExpRequiredForLevel(CurrentLevel+1);

		//Level up
		if(CurrentExperience >= nextExpRequired)
        {
			CurrentLevel++;
			numNewLevels++;
		
		//Level down
		}
        else if (CurrentExperience < currentExpRequired)
        {
			CurrentLevel--;
			numNewLevels--;
            
			if(numNewLevels < 0)
				numNewLevels = 0;
		}
	}

	
	public int GetExpRequiredForLevel(int lvl)
    {
		if(lvl <= 1)
			return 0;
		
		return (int)((Mathf.Pow(2, lvl) * 100f) + 100);
	}


    #endregion




    public override bool CanAfford(Cost _cost)
    {
        if (!base.CanAfford(_cost))
            return false;


        switch (_cost.Currency)
        {
            case CurrencyType.Experience:
                return CanModifyExp(_cost.Value);

            case CurrencyType.LevelPoints:
                return GameManager.Instance != null && GameManager.Instance.CanModifyLevelPoints(_cost.Value);
        }


        return true;
    }






    protected void PlayEffect(DisplayEffect _effect)
    {
        _effect.Play();
    }



    protected override void UpdateUI()
    {
        base.UpdateUI();

        UpdateHandheldProgressBar(m_HandheldItem == null ? 0f : m_HandheldItem.GetPercentage(),false);
        UpdateAbilityProgressBar(false);
        UpdateExperienceProgressBar();
    }
    protected void UpdateHandheldProgressBar(float percentage, bool setImmediate)
    {
        m_Handler.UpdateUI("Handheld", m_HandheldItem == null ? 0f : m_HandheldItem.GetPercentage(), setImmediate);
    }

    protected void UpdateAbilityProgressBar(bool setImmediate)
    {
        //base.UpdateAbilityProgressBar(NativeAbility, setImmediate);
        //base.UpdateAbilityProgressBar(AuxiliaryAbility, setImmediate);
    }

    protected void UpdateExperienceProgressBar()
    {
        //Debug.Log(string.Format("Current Exp: {0}. Exp Required for next level: {1}. Percentage: {2} %", CurrentExperience, GetExpRequiredForLevel(CurrentLevel + 1), CurrentExperienceLevelProgress * 100f));
        //UpdateExpBar(CurrentExperienceLevelProgress);
        m_Handler.UpdateUI(Attribute.Experience, CurrentExperienceLevelProgress, false);
    }
    protected void UpdateHandheldProgressBar()
    {
        m_Handler.UpdateUI("Handheld", m_HandheldItem == null ? 0f : m_HandheldItem.GetPercentage(), false);
    }









    public void CasualtyAchieved(Health _casualtyHealth)
    {
		if(_casualtyHealth.IsAlive)
        {
			if(nativeAbility != null)
				nativeAbility.DamageAchieved(_casualtyHealth.LastHealthChange);

			if(auxiliaryAbility != null)
				auxiliaryAbility.DamageAchieved(_casualtyHealth.LastHealthChange);

            //if(SoundManager.Instance != null)
            //	SoundManager.Instance.PlaySound(damageAchievedEffect);


            PlayEffect(damageAchievedEffect);
		}
        else
        {
			if(nativeAbility != null)
				nativeAbility.KillAchieved();

			if(auxiliaryAbility != null)
				auxiliaryAbility.KillAchieved();

            //if(SoundManager.Instance != null)
            //	SoundManager.Instance.PlaySound(killAchievedEffect);

            PlayEffect(killAchievedEffect);
		}
	}




    #region Accessors
    
    public string CharacterName
    {
        get { return characterName; }
    }

    public List<Stat> CurrentStats
    {
        get
        {
            List<Stat> _currentstats = new List<Stat>();

            StatType[] _types = Enum.GetValues(typeof(StatType)) as StatType[];

            for(int i = 0; i < _types.Length; i++)
            {
                Stat _stat = GetStat(_types[i]);

                if (_stat == null)
                    continue;

                _currentstats.Add(_stat);
            }


            return _currentstats;
        }
    }

    public bool ShouldCollectExp
    {
		get { return shouldCollectExp; }
		set { shouldCollectExp = value; }
	}
    public float ExpCollectRange
    {
        get { return expCollectRange; }
        private set { expCollectRange = Mathf.Clamp(value,0f,value); }
    }


    public int CurrentLevel
    {
        get { return currentLevel; }
        private set { currentLevel = Mathf.Clamp(value,1,value); }
    }
    public int CurrentExperience
    {
        get { return currentExp; }
        private set { currentExp = Mathf.Clamp(value,0,value); }
    }
    public float CurrentExperienceLevelProgress
    {
        get
        {
            int totalDiff = GetExpRequiredForLevel(CurrentLevel + 1) - GetExpRequiredForLevel(CurrentLevel);
            int curDiff = CurrentExperience - GetExpRequiredForLevel(CurrentLevel);
            
            return curDiff / (float)totalDiff;
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


    #region OnCollision / OnTrigger

    public virtual void OnTriggerEnter(Collider coll)
    {

        /*
		IInteractable _interactable = coll.GetComponent<IInteractable>();
		
		if(_interactable != null && _interactable.IsEnabled() && _interactable.IsUsableOutsideFOV()){
			AddInteractable(coll.gameObject);
		}*/

        if (!coll.isTrigger)
        {
            AttributeHandler _handler = coll.GetComponent<AttributeHandler>();
            if (_handler != null)
            {
                _handler.ShowUI(false);
            }
        }
    }

	public virtual void OnTriggerStay(Collider coll)
    {

		Vector3 toVector = coll.transform.position - m_Transform.position;


		//Debug.DrawLine(myTransform.position, coll.transform.position, Color.yellow);
		//Debug.DrawLine(myTransform.position, myTransform.position + (myTransform.forward * 5f), Color.cyan);


		ItemDrop _drop = coll.GetComponent<ItemDrop>();

		if(shouldCollectExp && _drop != null && _drop.Type == ItemDrop.ItemType.Experience)
        {
			Rigidbody _rigidbody = coll.GetComponent<Rigidbody>();
			
			if(toVector.magnitude <= ExpCollectRange && _rigidbody != null)
            {
				Vector3 forceVector = -toVector * EXP_COLLECT_SPEED;
				_rigidbody.AddForce(forceVector);
			}
		}




		InteractableObject _interactable = coll.GetComponent<InteractableObject>();
		
		if(_interactable != null && _interactable.IsUsable)
        {

			toVector.y = 0;

			if(toVector.magnitude <= InteractDistance && (_interactable.IsUsableOutsideFOV || Vector3.Angle(toVector, m_Transform.forward) <= FOV / 2f))
            {
				AddInteractable(coll.gameObject);
			}
            else
            {
				RemoveInteractable(coll.gameObject);
			}
		}


       
	}

	public virtual void OnTriggerExit(Collider coll)
    {
        InteractableObject _interactable = coll.GetComponent<InteractableObject>();
		
		if(_interactable != null)
			RemoveInteractable(coll.gameObject);


        if (!coll.isTrigger)
        {
            AttributeHandler _handler = coll.GetComponent<AttributeHandler>();
            if (_handler != null)
            {
                _handler.HideUI();
            }
        }
    }

	#endregion



    public bool Equals(PlayerController other)
    {
        return CharacterName.Equals(other.CharacterName);
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



    public void OnGUI()
    {
        if (!showDebug)
            return;


        Vector3 vel = m_Movement.Velocity;
        vel.y = 0;


        GUI.Label(new Rect(10, 10, 150, 20), "Current Speed: " + vel.magnitude);
    }

	public override void OnDrawGizmos()
    {
		base.OnDrawGizmos();

		if(showDebug && m_Transform != null)
        {

            if (m_Interactable != null && m_Interactable.gameObject.activeInHierarchy)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawLine(m_Transform.position, m_Interactable.transform.position);
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
        ExpCollectRange = ExpCollectRange;

        InteractDistance = InteractDistance;
        ThrowPower = ThrowPower;
	}
}

