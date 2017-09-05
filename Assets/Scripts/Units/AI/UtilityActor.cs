using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[RequireComponent(typeof(UtilityMind))]
public class UtilityActor : UnitController {

    static readonly float MINIMUM_UPDATE_TIME = 0.02f;


    //Percentage of UpdateTime to be used as sigma
    static readonly float UPDATE_PERCENTAGE_SIGMA = 0.5f;


    List<BaseUtilityBehavior> myBehaviors = new List<BaseUtilityBehavior>();

    BaseUtilityBehavior currentActiveBehavior;

    [Tooltip("String representation of current behavior heirarchy. For DEBUG purposes.")]
    [SerializeField]
    string currentBehaviorString;

    [Tooltip("Allowable amount of time to pass before checking for a new behavior")]
    [SerializeField]
    float updateBehaviorTime = 0.5f;

    bool recentlyUpdatedBehaviorFlag = false;



    [Tooltip("Poor decision response based on stupidity level")]
    [SerializeField]
    AnimationCurve poorDecisionCurve = AnimationCurve.Linear(0f, 0f, 100f, 100f);



    float stupidityLevel = 0f;

    //[Tooltip("Never lose track of an object in memory?")]
    //[SerializeField]
    //bool neverForget = false;


    [Tooltip("Amount of time an object remains in memory after losing track of it")]
    [SerializeField]
    float memoryTime = 0f;

    [Tooltip("Amount of time between seeing an object and reacting to it")]
    [SerializeField]
    float reactionTime = 0f;

    [Tooltip("Amount of time between checks for nearby objects")]
    [SerializeField]
    float checkNearbyTime = 0.01f;

    [Flags]
    public enum TargetingMethod { Random, MostHealth, LeastHealth, ShortestDistance, Custom, OnSight, OnSound, OnDamage }

    [Tooltip("TargetingMethod for targeting a NEW Enemy")]
    [SerializeField]
    [EnumFlags]
    TargetingMethod initialTargetingMethod;

    [Tooltip("TargetingMethod for targeting ANOTHER Enemy")]
    [SerializeField]
    [EnumFlags]
    TargetingMethod reTargetingMethod;

    [Tooltip("Chance to attempt targeting ANOTHER Enemy")]
    [SerializeField]
    [Range(0f, 1f)]
    float retargetChance = 0.5f;


    [Tooltip("Need to check if objects are obstacles are blocking it's vision?")]
    [SerializeField]
    bool canSeeThroughObjects = false;



    List<Transform> nearbyAllies = new List<Transform>();
    List<Transform> nearbyEnemies = new List<Transform>();

    List<Transform> nearbyWeapons = new List<Transform>();
    List<Transform> nearbyAbilities = new List<Transform>();

    List<Transform> nearbyHealth = new List<Transform>();
    List<Transform> nearbyExperience = new List<Transform>();
    List<IProjectile> nearbyProjectiles = new List<IProjectile>();

    //Linking of gameobject Tag to nearby Tranforms
    Dictionary<string, List<Transform>> nearbyTags = new Dictionary<string, List<Transform>>();

    //Track objects that are in sight
    Dictionary<Transform, SightedObject> objectsInSight = new Dictionary<Transform, SightedObject>();

    //Track objects that are being forgotten
    HashSet<GameObject> forgetSet = new HashSet<GameObject>();


    Transform targetTransform;
    Transform followTransform;



    [Tooltip("NodeTypes that Unit can walk over")]
    [SerializeField]
    [EnumFlags]
    protected NodeType walkableNodes = NodeType.BasicGround;




    //Pathfinding Variables
    [Tooltip("Ninimum distance to be considered close enough to A-Star Node")]
    [SerializeField]
    protected float closeEnoughToNode = 0.5f;
    [Tooltip("Allowable amount of time to pass before updating Path")]
    [SerializeField]
    protected float updatePathTime = 0.25f;

    List<Vector3> path = new List<Vector3>();
    int pathIndex = 0;

    bool searchingForPath = false;



    [Tooltip("Base Attack Power")]
    [SerializeField]
    int baseAttackPower = 10;
    float bonusAttackPower;

    [Tooltip("Base Attack Range")]
    [SerializeField]
    float baseAttackRange;
    float bonusAttackRange;

    [Tooltip("Base Attack Rate")]
    [SerializeField]
    float baseAttackRate;
    float bonusAttackRate;

    [Tooltip("Base Attack Speed")]
    [SerializeField]
    float baseAttackSpeed;
    float bonusAttackSpeed;

    [Tooltip("Base Critical Hit Chance")]
    [SerializeField]
    [Range(0f, 1f)]
    float baseCriticalHitChance = 0.2f;
    float bonusCriticalHitChance;

    [Tooltip("Base Critical Hit Multiplier")]
    [SerializeField]
    [Range(0f, 5f)]
    float baseCriticalHitMultiplier = 2;
    float bonusCriticalHitMultiplier;




    [Tooltip("Chance of levelling up HEALTH upon level-up")]
    [SerializeField]
    [Range(0f, 1f)]
    float chanceHealthLevelUp = 1f;

    [Tooltip("Chance of levelling up SPEED upon level-up")]
    [SerializeField]
    [Range(0f, 1f)]
    float chanceSpeedLevelUp = 1f;

    [Tooltip("Chance of levelling up DEXTERITY upon level-up")]
    [SerializeField]
    [Range(0f, 1f)]
    float chanceDexterityLevelUp = 1f;

    [Tooltip("Chance of levelling up DAMAGE upon level-up")]
    [SerializeField]
    [Range(0f, 1f)]
    float chanceDamageLevelUp = 1f;

    [Tooltip("Chance of levelling up CRITICAL HIT DAMAGE upon level-up")]
    [SerializeField]
    [Range(0f, 1f)]
    float chanceCriticalDamageLevelUp = 1f;

    [Tooltip("Chance of levelling up LUCK upon level-up")]
    [SerializeField]
    [Range(0f, 1f)]
    float chanceLuckLevelUp = 1f;

    [Tooltip("Amount to increase FOV upon level-up")]
    [SerializeField]
    AnimationCurve fovLevelUp = AnimationCurve.Linear(0f, 0f, 1f, 5f);

    [Tooltip("Trait effects upon encountering ALLY")]
    [SerializeField]
    List<TraitReaction> allyTraitReactions = new List<TraitReaction>();

    [Tooltip("Trait effects upon encountering ENEMY")]
    [SerializeField]
    List<TraitReaction> enemyTraitReactions = new List<TraitReaction>();

    [Tooltip("Trait effects upon encountering PROJECTILE")]
    [SerializeField]
    List<TraitReaction> projectileTraitReactions = new List<TraitReaction>();
    
    [Tooltip("Trait thresholds before unit is classified as an ENEMY")]
    [SerializeField]
    List<TraitThreshold> enemyTraitThresholds = new List<TraitThreshold>();

    [Serializable]
    struct TraitReaction
    {
        public UtilityPersonalityTrait.PersonalityTrait_Type traitType;
        public AnimationCurve sightCurve;
        public AnimationCurve hearingCurve;
        public AnimationCurve injuryCurve;


    

        public void Validate()
        {
            Utilities.ValidateCurve_Times(sightCurve, 0f,1f);
            Utilities.ValidateCurve_Times(hearingCurve, 0f, 1f);
            Utilities.ValidateCurve_Times(injuryCurve, 0f, 1f);
        }
    }

    [Serializable]
    struct TraitThreshold
    {
        public UtilityPersonalityTrait.PersonalityTrait_Type traitType;

        [SerializeField]
        [Range(0f, 100f)]
        public float threshold;
    }


    [SerializeField]
    bool showTextUI = false;

    [SerializeField]
    Vector3 textUIOffset = Vector3.zero;


    GenericUI m_TextUI;
    UtilityMind m_Mind;



    public delegate void AlertEvent(Transform t);
    public AlertEvent OnDamage;
    public AlertEvent OnSight;
    public AlertEvent OnSound;





    public override void Awake()
    {
        base.Awake();

        m_Health.OnDamaged += UnitDamaged;


        m_Mind = GetComponent<UtilityMind>();
    }
    public override void Start()
    {
        base.Start();


        if (GameManager.Instance != null)
        {
            m_Health.OnKilled += GameManager.Instance.EnemyKilled;
        }


        ScaleAI();

    }
    public void OnEnable()
    {
        GatherAllBehaviors();


        StartCoroutine(UpdateBehavior());
        StartCoroutine(UpdateTraits());

        StartCoroutine(CheckNearbyRoutine());
    }
    public override void OnDisable()
    {
        base.OnDisable();

        StopAllCoroutines();

        if(m_TextUI != null)
        {
            m_TextUI.enabled = false;
        }
        //DeflateUtilityUI();
    }


    #region Behavior Stuff

    /// <summary>
    /// Choose a NEW Behavior to activate. Uses poorDecisionCurve.
    /// </summary>
    public void ChooseNewBehavior()
    {
        float percentToConsider = poorDecisionCurve.Evaluate(stupidityLevel);

        ChooseNewBehavior(percentToConsider);
    }

    /// <summary>
    /// Choose a NEW Behavior to activate. Chooses best scoring behavior.
    /// </summary>
    public void ChooseBestBehavior()
    {
        ChooseNewBehavior(0f);
    }

    /// <summary>
    /// Choose a NEW Behavior to activate
    /// </summary>
    /// <param name="percentToConsider"> Percentage of all possible behaviors to consider</param>
    public void ChooseNewBehavior(float percentToConsider)
    {
        if (ShowDebug)
        {
            Debug.Log(Time.time + " #### " + m_Transform.name + " -- ChooseNewBehavior(). Behavior count: " + myBehaviors.Count);
        }


        if (myBehaviors.Count == 0)
            return;

        if (currentActiveBehavior != null && !currentActiveBehavior.CanEndBehavior)
        {
            if (ShowDebug)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + " -- Can't end current behavior. Current Behavior: " + CurrentBehavior.ToString());
            }


            return;
        }

        List<WeightedObjectOfUtilityBehavior> weightedBehaviors = new List<WeightedObjectOfUtilityBehavior>();


        for (int i = 0; i < myBehaviors.Count; i++)
        {
            //if (myBehaviors[i].IsActive)
            //{
            //    Debug.Log(Time.time + " #### " + m_Transform.name + ". Behavior is active so it will not be added to potential list. Behavior: " + myBehaviors[i].ToString());
            //    continue;
            //}

            if (!myBehaviors[i].IsActive && !myBehaviors[i].CanStartBehavior)
            {
                if (ShowDebug)
                {
                    Debug.Log(Time.time + " #### " + m_Transform.name + ". Behavior cannot be started so it will not be added to potential list. Behavior: " + myBehaviors[i].ToString());
                }


                continue;
            }

            float _weight = myBehaviors[i].GetBehaviorScore();

            if (_weight <= 0)
            {
                if (ShowDebug)
                {
                    Debug.Log(Time.time + " #### " + m_Transform.name + ". Behavior's weight is not greater than 0 so it will not be added to potential list. Behavior: " + myBehaviors[i].ToString());
                }

                continue;
            }


            weightedBehaviors.Add(new WeightedObjectOfUtilityBehavior(myBehaviors[i], _weight));

        }


        if (weightedBehaviors.Count == 0)
        {
            if (ShowDebug)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + " -- No weighted behaviors available.");
            }


            return;
        }



        weightedBehaviors.Sort(delegate (WeightedObjectOfUtilityBehavior x, WeightedObjectOfUtilityBehavior y)
        {
            return y.Item2.CompareTo(x.Item2);
        });




        List<WeightedObjectOfUtilityBehavior> considerationBehaviors = new List<WeightedObjectOfUtilityBehavior>();

        int numToConsider = (int)(percentToConsider * weightedBehaviors.Count);
        numToConsider = numToConsider == 0 ? 1 : numToConsider;


  
        for (int i = 0; i < weightedBehaviors.Count && i < numToConsider; i++)
        {
            if (weightedBehaviors[i].Item2 <= 0)
                continue;

            if (ShowDebug)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + " Adding potential behavior. Behavior: " + weightedBehaviors[i].Item1 + ". Score: " + weightedBehaviors[i].Item2);
            }


            considerationBehaviors.Add(weightedBehaviors[i]);
        }


        if (considerationBehaviors.Count == 0)
            return;


        BaseUtilityBehavior chosenBehavior = Utilities.WeightedSelection<BaseUtilityBehavior>(considerationBehaviors.ToArray(), 0f);

        if (chosenBehavior == currentActiveBehavior)
        {
            if (ShowDebug)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + " Chosen behavior is already current active behavior: " + currentActiveBehavior.ToString());
            }

            return;
        }

        if (ShowDebug)
        {
            if (currentActiveBehavior != null)
            {
                Debug.Log(Time.time + " #### " + m_Transform.name + " -- Ending Behavior -- " + currentActiveBehavior.ToString());
            }


            float _score = -1f;

            for (int i = 0; i < considerationBehaviors.Count; i++)
            {
                if (considerationBehaviors[i].Item1 == chosenBehavior)
                {
                    _score = considerationBehaviors[i].Item2;
                }
                else
                {
                    Debug.Log(Time.time + " #### "+ m_Transform.name + " -- Potential Behavior -- " + considerationBehaviors[i].ToString() + " ## Score: " + considerationBehaviors[i].Item2);
                }
            }

            Debug.Log(Time.time + " #### " + m_Transform.name + " -- New Behavior -- " + chosenBehavior.ToString() + " ## Score: " + _score);
        }





        if (currentActiveBehavior == null )
        {
            CurrentBehavior = chosenBehavior;
            currentActiveBehavior.StartBehavior();
        }
        else if (currentActiveBehavior.IsActive && currentActiveBehavior.CanStartSubBehavior && currentActiveBehavior.TryStartSubBehavior(chosenBehavior))
        {
            // isNewBehavior = !currentBehavior.TryStartSubBehavior(chosenBehavior);

            // Debug.Log("Tried StartingSubBehavior -- " + currentBehavior + ". Sub: " + chosenBehavior + " ## " + isNewBehavior);
        }
        else if (!chosenBehavior.NeedsSuperBehavior)
        {
            currentActiveBehavior.EndBehavior(false, false);
            CurrentBehavior = chosenBehavior;
            currentActiveBehavior.StartBehavior();
        }


        recentlyUpdatedBehaviorFlag = true;
    }

    /// <summary>
    /// Add behavior to list of behaviors to check
    /// </summary>
    public void AddBehavior(BaseUtilityBehavior _behavior)
    {
        myBehaviors.Add(_behavior);
    }

    /// <summary>
    /// Remove behavior from list of behaviors to check
    /// </summary>
    public bool RemoveBehavior(BaseUtilityBehavior _behavior)
    {
        int index = -1;

        for (int i = 0; i < myBehaviors.Count; i++)
        {
            if (myBehaviors[i] == _behavior)
            {
                index = i;
                break;
            }
        }


        if (index == -1)
            return false;


        myBehaviors.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Gather list of all behaviors to check
    /// </summary>
    public void GatherAllBehaviors()
    {
        myBehaviors.Clear();

        BaseUtilityBehavior[] _behaviors = GetComponents<BaseUtilityBehavior>();

        for(int i = 0; i < _behaviors.Length; i++)
        {
            AddBehavior(_behaviors[i]);
        }
    }

    /// <summary>
    /// Inform UtilityActor that behavior ended
    /// </summary>
    public void NotifyBehaviorEnded(BaseUtilityBehavior _behavior)
    {

        if (_behavior != currentActiveBehavior)
            return;

        if (currentActiveBehavior.IsActive)
            currentActiveBehavior.EndBehavior(false, false);


        if (showDebug)
        {
            Debug.Log(Time.time + " #### " + m_Transform.name + " -- Behavior Ended -- " + _behavior.ToString());
        }

        currentActiveBehavior = null;

        ChooseNewBehavior();
        recentlyUpdatedBehaviorFlag = true;
    }

    /// <summary>
    /// Check for new behavior consistently
    /// </summary>
    IEnumerator UpdateBehavior()
    {
        while (true)
        {
            yield return new WaitForSeconds(UpdateBehaviorTime);


            if (!recentlyUpdatedBehaviorFlag)
                ChooseNewBehavior();


            recentlyUpdatedBehaviorFlag = false;
        }
    }

    #endregion





    #region Stupidity Stuff

    /// <summary>
    /// Add to stupidityLevel
    /// </summary>
    /// <param name="delta"> Amount to change </param>
    public void StupidityArithmetic(float delta)
    {
        stupidityLevel = Mathf.Clamp(stupidityLevel + delta, 0f, 100f);
    }

    /// <summary>
    /// Set stupidityLevel
    /// </summary>
    /// <param name="amt"> New stupidityLevel</param>
    public void SetStupidityLevel(float amt)
    {
        StupidityArithmetic(amt - stupidityLevel);
    }

    #endregion


    #region Path Following


    public bool MoveAlongPath()
    {
        return MoveAlongPath(true);
    }


    /// <summary>
    /// Attempt to move along current PATH
    /// </summary>
    public bool MoveAlongPath(bool shouldFaceDirection)
    {
        if (searchingForPath)
            return true;


        if (Path == null || Path.Count == 0 || pathIndex >= Path.Count)
            return false;

        int index = pathIndex;

        Vector3 moveVector = Path[pathIndex] - m_Transform.position;
        //moveVector.y = 0;

        if (moveVector.magnitude <= closeEnoughToNode)
        {
            pathIndex++;
        }

        Move(Path[index], shouldFaceDirection);

        return true;
    }

    protected virtual void Move(Vector3 desiredPos, bool shouldFaceDirection)
    {

        desiredPos.y += Utilities.CalculateObjectBounds(gameObject, false).y / 2;

        Vector3 moveVector = desiredPos - m_Transform.position;
        moveVector.y = 0;

        //Debug.Log(string.Format("Move vector magnitude {0}", moveVector.magnitude));

        //  myMovement.RotateTowards(desiredPos);// myTransform.position + moveVector.normalized);
        // myMovement.Move(moveVector.normalized);

        m_Movement.MoveTowards(desiredPos);
    }

    /// <summary>
    /// Request Path to position
    /// </summary>
    public void FindPathTo(Transform t)
    {
        FindPathTo(t.position);
    }
    public void FindPathTo(Vector3 worldPos)
    {
        searchingForPath = true;

        Vector3 bounds = Utilities.CalculateObjectBounds(gameObject, false);
        //A_Star_Pathfinding.Instance.FindPath(myTransform.position, worldPos, bounds, walkableNodes, SetPath); //InsertInQueue(myTransform.position, worldPos, SetPath);
        A_Star_Pathfinding.Instance.InsertInQueue(m_Transform.position, worldPos, bounds, walkableNodes, SetPath);
    }


    /// <summary>
    /// Set PATH. To be used by A-Star
    /// </summary>
    public void SetPath(List<Vector3> newPath)
    {
        if (newPath == null)
        {
            Debug.Log("Null path returned");
            return;
        }


        path = newPath;
        pathIndex = 0;

        searchingForPath = false;
    }

    /// <summary>
    /// Clear PATH
    /// </summary>
    public void ClearPath()
    {
        path = new List<Vector3>();
        pathIndex = 0;
    }
    #endregion


    #region Stat Stuff


    /// <summary>
    /// Scale AI to Current Level
    /// </summary>
    public void ScaleAI()
    {
        if (GameManager.Instance == null)
            return;



        float bonusFOV = 0f;

        for (int i = 1; i < GameManager.Instance.CurrentLevel; i++)
        {
            if (UnityEngine.Random.value <= chanceHealthLevelUp)
            {
                ChangeStat(StatType.Health, 1);
            }

            if (UnityEngine.Random.value <= chanceSpeedLevelUp)
            {
                ChangeStat(StatType.Speed, 1);
            }

            if (UnityEngine.Random.value <= chanceDexterityLevelUp)
            {
                ChangeStat(StatType.Dexterity, 1);
            }

            if (UnityEngine.Random.value <= chanceDamageLevelUp)
            {
                ChangeStat(StatType.Damage, 1);
            }

            if (UnityEngine.Random.value <= chanceCriticalDamageLevelUp)
            {
                ChangeStat(StatType.CriticalDamage, 1);
            }

            if (UnityEngine.Random.value <= chanceLuckLevelUp)
            {
                ChangeStat(StatType.Luck, 1);
            }

            bonusFOV += fovLevelUp.Evaluate(UnityEngine.Random.value);
        }

        UpdateAllStatEffects();
        FOV += (int)bonusFOV;

    }


    protected override void UpdateStatEffects(StatType _type)
    {
        base.UpdateStatEffects(_type);

        Stat _stat = GetStat(_type);

        if (_stat == null)
            return;


        switch (_stat.Type)
        {
            case StatType.Dexterity:
                BonusAttackRate = _stat.CurrentValue;
                BonusAttackSpeed = _stat.CurrentValue;
                break;
            case StatType.Damage:
                BonusAttackPower = (int)_stat.CurrentValue;
                break;
            case StatType.CriticalDamage:
                BonusCriticalHitMultiplier = _stat.CurrentValue;

                break;
            case StatType.Luck:
                BonusCriticalHitChance = _stat.CurrentValue;

                break;
            default:
                break;
        }
    }

    #endregion


    protected void PlaySound(AudioClip _sound)
    {
        m_Audio.Stop();

        if (_sound != null)
            m_Audio.PlayOneShot(_sound);
    }



    #region Mind Stuff

    public void ChangeTrait(UtilityPersonalityTrait.PersonalityTrait_Type _type, float _delta, Transform _influencer)
    {
        m_Mind.ModifyTrait(_type, _delta, _influencer);
    }

    /// <summary>
    /// React to Damage
    /// </summary>
    public override void UnitDamaged(Health _health)
    {
        base.UnitDamaged(_health);

        float pctg = Mathf.Clamp01(Mathf.Abs(_health.LastHealthChange / _health.MaxHealth));

        //Allies
        for (int i = 0; i < nearbyAllies.Count; i++)
        {
            if (!nearbyAllies[i] == _health.LastAttacker)
                continue;


            for (int k = 0; k < allyTraitReactions.Count; k++)
            {
                float traitDelta = allyTraitReactions[k].injuryCurve.Evaluate(pctg);

                ChangeTrait(allyTraitReactions[i].traitType, traitDelta, nearbyAllies[i]);
            }
        }

        //Enemies
        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (!nearbyEnemies[i] == _health.LastAttacker)
                continue;


            for (int k = 0; k < enemyTraitReactions.Count; k++)
            {
                float traitDelta = enemyTraitReactions[k].injuryCurve.Evaluate(pctg);

                ChangeTrait(enemyTraitReactions[i].traitType, traitDelta, nearbyEnemies[i]);
            }
        }


        if (TargetTransform == null && Utilities.HasFlag(initialTargetingMethod, TargetingMethod.OnDamage))
        {
            CheckTarget(m_Health.LastAttacker);
        }
        else if (TargetTransform != null && Utilities.HasFlag(reTargetingMethod, TargetingMethod.OnDamage))
        {
            CheckRetarget(m_Health.LastAttacker);
        }
    }

    /// <summary>
    /// React to Death
    /// </summary>
    public override void UnitKilled(Health _casualtyHealth)
    {
        base.UnitKilled(_casualtyHealth);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyKilled(gameObject);
        }
    }

    #endregion


    #region Target Selection

    public Transform TargetEnemy_Random()
    {
        List<Transform> validEnemies = new List<Transform>();


        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (!nearbyEnemies[i].gameObject.activeInHierarchy)
            {
                nearbyEnemies.RemoveAt(i);
                i--;
                continue;
            }

            if (!EnemyTraitThresholdCheck(nearbyEnemies[i]))
                continue;

            validEnemies.Add(nearbyEnemies[i]);
        }

        if (validEnemies.Count == 0)
            return null;


        int index = UnityEngine.Random.Range(0, validEnemies.Count);

        return nearbyEnemies[index];
    }
    public Transform TargetEnemy_ShortestDistance()
    {

        List<CustomTuple2<Transform, float>> validEnemies = new List<CustomTuple2<Transform, float>>();


        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (nearbyEnemies[i] == null || !nearbyEnemies[i].gameObject.activeInHierarchy)
            {
                nearbyEnemies.RemoveAt(i);
                i--;
                continue;
            }

            if (!EnemyTraitThresholdCheck(nearbyEnemies[i]))
                continue;

            float dist = A_Star_Pathfinding.Instance.EstimatePathDistance(m_Transform.position, nearbyEnemies[i].position, Utilities.CalculateObjectBounds(gameObject, false), WalkableNodes);

            validEnemies.Add(new CustomTuple2<Transform, float>(nearbyEnemies[i], dist));

        }

        if (validEnemies.Count == 0)
            return null;


        validEnemies.Sort(delegate (CustomTuple2<Transform, float> x, CustomTuple2<Transform, float> y)
        {
            return x.Item2.CompareTo(y.Item2);
        });





        List<WeightedObject<Transform>> weightedDistances = new List<WeightedObject<Transform>>();

        int numToConsider = (int)(poorDecisionCurve.Evaluate(stupidityLevel));
        numToConsider = numToConsider == 0 ? 1 : numToConsider;

        for (int i = 0; i < numToConsider; i++)
        {
            weightedDistances.Add(new WeightedObject<Transform>(validEnemies[i].Item1, validEnemies[i].Item2));
        }

        return Utilities.WeightedSelection<Transform>(weightedDistances.ToArray(), 0f);
    }
    public Transform TargetEnemy_MostHealth()
    {

        List<CustomTuple2<Transform, float>> validEnemies = new List<CustomTuple2<Transform, float>>();


        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (nearbyEnemies[i] == null || !nearbyEnemies[i].gameObject.activeInHierarchy)
            {
                nearbyEnemies.RemoveAt(i);
                i--;
                continue;
            }

            if (!EnemyTraitThresholdCheck(nearbyEnemies[i]))
                continue;

            Health _health = nearbyEnemies[i].GetComponent<Health>();

            if (_health == null)
                continue;

            validEnemies.Add(new CustomTuple2<Transform, float>(nearbyEnemies[i], _health.CurHealth));

        }

        if (validEnemies.Count == 0)
            return null;


        validEnemies.Sort(delegate (CustomTuple2<Transform, float> x, CustomTuple2<Transform, float> y)
        {
            return y.Item2.CompareTo(x.Item2);
        });





        List<WeightedObject<Transform>> weightedDistances = new List<WeightedObject<Transform>>();

        int numToConsider = (int)(poorDecisionCurve.Evaluate(stupidityLevel));
        numToConsider = numToConsider == 0 ? 1 : numToConsider;

        for (int i = 0; i < numToConsider; i++)
        {
            weightedDistances.Add(new WeightedObject<Transform>(validEnemies[i].Item1, validEnemies[i].Item2));
        }

        return Utilities.WeightedSelection<Transform>(weightedDistances.ToArray(), 0f);

    }
    public Transform TargetEnemy_LeastHealth()
    {

        List<CustomTuple2<Transform, float>> validEnemies = new List<CustomTuple2<Transform, float>>();


        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (nearbyEnemies[i] == null || !nearbyEnemies[i].gameObject.activeInHierarchy)
            {
                nearbyEnemies.RemoveAt(i);
                i--;
                continue;
            }

            if (!EnemyTraitThresholdCheck(nearbyEnemies[i]))
                continue;

            Health _health = nearbyEnemies[i].GetComponent<Health>();

            if (_health == null)
                continue;

            validEnemies.Add(new CustomTuple2<Transform, float>(nearbyEnemies[i], _health.CurHealth));

        }

        if (validEnemies.Count == 0)
            return null;


        validEnemies.Sort(delegate (CustomTuple2<Transform, float> x, CustomTuple2<Transform, float> y)
        {
            return x.Item2.CompareTo(y.Item2);
        });





        List<WeightedObject<Transform>> weightedDistances = new List<WeightedObject<Transform>>();

        int numToConsider = (int)(poorDecisionCurve.Evaluate(stupidityLevel));
        numToConsider = numToConsider == 0 ? 1 : numToConsider;

        for (int i = 0; i < numToConsider; i++)
        {
            weightedDistances.Add(new WeightedObject<Transform>(validEnemies[i].Item1, validEnemies[i].Item2));
        }

        return Utilities.WeightedSelection<Transform>(weightedDistances.ToArray(), 0f);

    }
    


    public Transform ChooseTarget(TargetingMethod _method)
    {
        List<Transform> possibleTargets = new List<Transform>();
        Transform t;

        
        if(Utilities.HasFlag(_method, TargetingMethod.LeastHealth))
        {
            t = TargetEnemy_LeastHealth();

            if (t != null)
            {
                possibleTargets.Add(t);
            }
        }

        if (Utilities.HasFlag(_method, TargetingMethod.MostHealth))
        {
            t = TargetEnemy_MostHealth();

            if (t != null)
            {
                possibleTargets.Add(t);
            }
        }
      
        if (Utilities.HasFlag(_method, TargetingMethod.Random))
        {
            t = TargetEnemy_Random();

            if (t != null)
            {
                possibleTargets.Add(t);
            }
        }

        if (Utilities.HasFlag(_method, TargetingMethod.ShortestDistance))
        {
            t = TargetEnemy_ShortestDistance();

            if (t != null)
            {
                possibleTargets.Add(t);
            }
        }
        

        if(possibleTargets.Count == 0)
        {
            return null;
        }


        return possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
    }


    private void CheckTarget(Transform t)
    {
        if (t == null)
            return;

        Health h = t.GetComponent<Health>();

        if (h == null || !h.IsAlive)
            return;
        

        TargetTransform = t;
    }
    private void CheckRetarget(Transform t)
    {
        if (UnityEngine.Random.value > retargetChance)
            return;

        CheckTarget(t);
    }


    bool EnemyTraitThresholdCheck(Transform _enemy)
    {
        bool isValid = true;

        for (int i = 0; i < enemyTraitThresholds.Count; i++)
        {
            UtilityPersonalityTrait _trait = m_Mind.GetTrait(enemyTraitThresholds[i].traitType);

            if (_trait == null)
                continue;

            if (_trait.GetInfluenceAmount(_enemy) < enemyTraitThresholds[i].threshold)
                isValid = false;
        }

        return isValid;
    }

    #endregion


    #region Entity Memory

    public bool IsAlly(Collider coll)
    {
        if (coll.isTrigger)
            return false;


        return Utilities.IsInLayerMask(coll.gameObject, friendlyMask) && allyTags.Contains(coll.gameObject.tag);
    }
    public bool IsEnemy(Collider coll)
    {
        if (coll.isTrigger)
            return false;


        return Utilities.IsInLayerMask(coll.gameObject, enemyMask) && !allyTags.Contains(coll.gameObject.tag);
    }

    public bool IsWeapon(Collider coll)
    {
        return coll.gameObject.GetComponent<Weapon>() != null;
    }
    public bool IsAbility(Collider coll)
    {
        return coll.gameObject.GetComponent<Ability>() != null;
    }

    public bool IsHealth(Collider coll)
    {
        return coll.gameObject.GetComponent<IHealthProvider>() != null;
    }
    public bool IsExperience(Collider coll)
    {
        return coll.gameObject.GetComponent<IExperienceProvider>() != null;
    }
    public bool IsProjectile(Collider coll)
    {
        return coll.gameObject.GetComponent<IProjectile>() != null;
    }




    IEnumerator ForgetObject(GameObject objToForget)
    {
        Transform objTransform = objToForget.transform;


        forgetSet.Add(objToForget);

        if (objectsInSight.ContainsKey(objTransform))
        {
            objectsInSight[objTransform].InSight = false;
        }

        if (memoryTime > 0)
        {
            yield return new WaitForSeconds(memoryTime);
        

            for (int i = 0; i < nearbyAllies.Count; i++)
            {
                if (nearbyAllies[i] == objTransform)
                {
                    nearbyAllies.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < nearbyEnemies.Count; i++)
            {
                if (nearbyEnemies[i] == objTransform)
                {
                    nearbyEnemies.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < nearbyWeapons.Count; i++)
            {
                if (nearbyWeapons[i] == objTransform)
                {
                    nearbyWeapons.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < nearbyAbilities.Count; i++)
            {
                if (nearbyAbilities[i] == objTransform)
                {
                    nearbyAbilities.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < nearbyHealth.Count; i++)
            {
                if (nearbyHealth[i] == objTransform)
                {
                    nearbyHealth.RemoveAt(i);
                    break;
                }
            }


            for (int i = 0; i < nearbyExperience.Count; i++)
            {
                if (nearbyExperience[i] == objTransform)
                {
                    nearbyExperience.RemoveAt(i);
                    break;
                }
            }


            /*
            if (nearbyWeapons.Contains(objTransform))
                nearbyWeapons.Remove(objTransform);

            if (nearbyAbilities.Contains(objTransform))
                nearbyAbilities.Remove(objTransform);



            if (nearbyHealth.Contains(objTransform))
                nearbyHealth.Remove(objTransform);

            if (nearbyExperience.Contains(objTransform))
                nearbyExperience.Remove(objTransform);
                */


            IProjectile pScript = objTransform.GetComponent<IProjectile>();
            if (pScript != null && nearbyProjectiles.Contains(pScript))
                nearbyProjectiles.Remove(pScript);

            if (objectsInSight.ContainsKey(objTransform))
            {
                objectsInSight.Remove(objTransform);
            }

            if (TargetTransform == objTransform)
                TargetTransform = null;
        }


        if (forgetSet.Contains(objToForget))
            forgetSet.Remove(objToForget);
    }
    #endregion




    void InflateUtilityUI()
    {
        if (m_TextUI != null && m_TextUI.gameObject.activeInHierarchy && m_TextUI.TargetTransform == m_Transform)
        {
            m_TextUI.SetFollowOffset(Vector3.zero);
        }
        else
        {
            GameObject uiObj = ObjectPoolerManager.Instance.InteractableUIPooler.GetPooledObject();

            if (uiObj == null)
                return;

            if (m_Transform == null)
                m_Transform = GetComponent<Transform>();


            m_TextUI = uiObj.GetComponent<GenericUI>();

            uiObj.transform.position = transform.position;// + (Vector3.up * UI_START_HEIGHT_OFFSET);
            uiObj.SetActive(true);
            m_TextUI.Initialize(m_Transform, true);


            GameObject _text = m_TextUI.GetPrefab("ID");

            if (_text != null)
            {
                DisplayUI _ui = _text.GetComponent<DisplayUI>();
                _text.SetActive(true);


                m_TextUI.AddAttribute(new GenericUI.DisplayProperties("ID", new Orientation(textUIOffset, Vector3.zero, Vector3.one), _ui));
                m_TextUI.UpdateAttribute("ID", CurrentBehaviorString);
            }



            Transform _transform = m_TextUI.GetParentTransform("Charges");

            if (_transform != null)
            {
                _transform.gameObject.SetActive(false);
            }
        }
    }
    public void DeflateUtilityUI()
    {
        if (m_TextUI == null || !m_TextUI.gameObject.activeInHierarchy || m_TextUI.TargetTransform != m_Transform)
            return;

        //m_TextUI.SetFollowOffset(Vector3.up * UI_START_HEIGHT_OFFSET);
        m_TextUI.Deflate();
    }





    #region Getters / Setters


    public BaseUtilityBehavior CurrentBehavior
    {
        get { return currentActiveBehavior; }
        set
        {
            currentActiveBehavior = value;

            currentBehaviorString = "";



            if (currentActiveBehavior != null)
            {
                currentBehaviorString = currentActiveBehavior.ToString();

                BaseUtilityBehavior _behavior = currentActiveBehavior.SubBehavior;
                while (_behavior != null)
                {
                    currentBehaviorString += "-->" + _behavior.ToString();
                    _behavior = _behavior.SubBehavior;
                }
            }


            if (showTextUI)
            {
                InflateUtilityUI();
                m_TextUI.UpdateAttribute("ID", CurrentBehaviorString);
            }
            else
            {
                DeflateUtilityUI();
            }
        }
    }
    public string CurrentBehaviorString
    {
        get { return currentBehaviorString; }
    }

    public int AttackPower
    {
        get { return (int)(baseAttackPower * (BonusAttackPower + 1)); }
    }
    public float AttackRange
    {
        get { return baseAttackRange * (BonusAttackPower + 1); }
    }
    public float AttackRate
    {
        get { return baseAttackRate * (BonusAttackRate + 1); }
    }
    public float AttackSpeed
    {
        get { return baseAttackSpeed * (BonusAttackSpeed + 1); }
    }

    public float CriticalAttackPower
    {
        get { return AttackPower * CriticalHitMultiplier; }
    }
    public float CriticalHitMultiplier
    {
        get { return baseCriticalHitMultiplier + BonusCriticalHitMultiplier; }
    }
    public float CriticalHitChance
    {
        get { return baseCriticalHitChance + BonusCriticalHitChance; }
    }



    public float BonusAttackPower
    {
        get { return bonusAttackPower; }
        set { bonusAttackPower = value; }
    }
    public float BonusAttackRange
    {
        get { return bonusAttackRange; }
        set { bonusAttackRange = value; }
    }
    public float BonusAttackRate
    {
        get { return bonusAttackRate; }
        set { bonusAttackRate = value; }
    }
    public float BonusAttackSpeed
    {
        get { return bonusAttackSpeed; }
        set { bonusAttackSpeed = value; }
    }
    public float BonusCriticalHitMultiplier
    {
        get { return bonusCriticalHitMultiplier; }
        set { bonusCriticalHitMultiplier = value; }
    }
    public float BonusCriticalHitChance
    {
        get { return bonusCriticalHitChance; }
        set { bonusCriticalHitChance = value; }
    }


    public Vector3 Bounds
    {
        get { return Utilities.CalculateObjectBounds(this.gameObject, false); }
    }
    public NodeType WalkableNodes
    {
        get { return walkableNodes; }
    }


    public float UpdatePathTime
    {
        get
        {
            float val = (float)Utilities.GetRandomGaussian(updatePathTime, updatePathTime * UPDATE_PERCENTAGE_SIGMA);

            val = val <= 0 ? MINIMUM_UPDATE_TIME : val;

            return val;
        }
    }
    public float UpdateBehaviorTime
    {
        get
        {
            float val = (float)Utilities.GetRandomGaussian(updateBehaviorTime, updateBehaviorTime * UPDATE_PERCENTAGE_SIGMA);

            val = val <= 0 ? MINIMUM_UPDATE_TIME : val;

            return val;
        }
    }



    public float MemoryTime
    {
        get { return memoryTime; }
        private set
        {
            memoryTime = value;

            if( memoryTime < -1)
            {
                memoryTime = -1;
            }
        }
    }
    public float ReactionTime
    {
        get { return reactionTime; }
    }



    public List<Vector3> Path
    {
        get { return path != null ? path : new List<Vector3>(); }
    }
    public bool SearchingForPath
    {
        get { return searchingForPath; }
    }



    public SightedObject TargetObject
    {
        get
        {
            //Should check and possibly retarget?
            if (TargetTransform != null && TargetTransform.gameObject.activeInHierarchy && EnemyTraitThresholdCheck(TargetTransform))
            {
                if (UnityEngine.Random.value < retargetChance)
                {
                    Transform t = ChooseTarget(reTargetingMethod);

                    if(t != null)
                    {
                        TargetTransform = t;
                    }
                }
            }
            //Initial target
            else
            {
                Transform t = ChooseTarget(initialTargetingMethod);

                if (t != null)
                {
                    TargetTransform = t;
                }
            }

            if (TargetTransform == null || !objectsInSight.ContainsKey(TargetTransform))
                return null;

            return objectsInSight[TargetTransform];
        }
    }

    public Transform TargetTransform
    {
        get { return targetTransform; }
        set
        {
            targetTransform = value;
        }
    }


    public SightedObject FollowTarget
    {
        get
        {
            if (followTransform == null || !objectsInSight.ContainsKey(followTransform))
                return null;

            return objectsInSight[followTransform];
        }
        //set { followTransform = value; }
    }
    public Transform FollowTransform
    {
        set { followTransform = value; }
    }


    public List<Transform> NearbyAllies
    {
        get
        { 
            for(int i = 0; i < nearbyAllies.Count; i++)
            {
                if(nearbyAllies[i] == null || !nearbyAllies[i].gameObject.activeInHierarchy)
                {
                    nearbyAllies.RemoveAt(i);
                    i--;
                }
            }
            return nearbyAllies;
        }
    }
    public List<Transform> NearbyEnemies
    {
        get
        {
            for (int i = 0; i < nearbyEnemies.Count; i++)
            {
                if (nearbyEnemies[i] == null || !nearbyEnemies[i].gameObject.activeInHierarchy)
                {
                    nearbyEnemies.RemoveAt(i);
                    i--;
                }
            }

            return nearbyEnemies;
        }
    }
    public List<Transform> NearbyWeapons
    {
        get
        {

            for (int i = 0; i < nearbyWeapons.Count; i++)
            {
                if (nearbyWeapons[i] == null || !nearbyWeapons[i].gameObject.activeInHierarchy)
                {
                    nearbyWeapons.RemoveAt(i);
                    i--;
                }
            }
            return nearbyWeapons;
        }
    }
    public List<Transform> NearbyAbilities
    {
        get
        {

            for (int i = 0; i < nearbyAbilities.Count; i++)
            {
                if (nearbyAbilities[i] == null || !nearbyAbilities[i].gameObject.activeInHierarchy)
                {
                    nearbyAbilities.RemoveAt(i);
                    i--;
                }
            }

            return nearbyAbilities;
        }
    }
    public List<Transform> NearbyHealth
    {
        get
        {
            for (int i = 0; i < nearbyHealth.Count; i++)
            {
                if (nearbyHealth[i] == null || !nearbyHealth[i].gameObject.activeInHierarchy)
                {
                    nearbyHealth.RemoveAt(i);
                    i--;
                }
            }
            return nearbyHealth;
        }
    }
    public List<Transform> NearbyExperience
    {
        get
        {
            for (int i = 0; i < nearbyExperience.Count; i++)
            {
                if (nearbyExperience[i] == null || !nearbyExperience[i].gameObject.activeInHierarchy)
                {
                    nearbyExperience.RemoveAt(i);
                    i--;
                }
            }

            return nearbyExperience;
        }
    }
    public List<IProjectile> NearbyProjectiles
    {
        get
        {
            for (int i = 0; i < nearbyProjectiles.Count; i++)
            {
                GameObject g;
                if (nearbyProjectiles[i] == null || (g = nearbyProjectiles[i].GameObject) == null || g.activeInHierarchy)
                {
                    nearbyProjectiles.RemoveAt(i);
                    i--;
                }
            }

            return nearbyProjectiles;
        }
    }



    public float StupidityLevel
    {
        get { return stupidityLevel; }
    }

    #endregion



    IEnumerator UpdateTraits()
    {
        while (true)
        {
            yield return null;


            //Allies
            for(int i = 0; i < nearbyAllies.Count; i++)
            {

                float distPercentage = Vector3.Distance(m_Transform.position, objectsInSight[nearbyAllies[i]].LastKnownPosition) / SightRange;

                if (distPercentage > 1f)
                    continue;

                for(int k = 0; k < allyTraitReactions.Count; k++)
                {
                    float traitDelta = allyTraitReactions[k].sightCurve.Evaluate(distPercentage) * Time.deltaTime;

                    ChangeTrait(allyTraitReactions[k].traitType, traitDelta, nearbyAllies[i]);
                }
            }

            //Enemies
            for (int i = 0; i < nearbyEnemies.Count; i++)
            {
                float distPercentage = Vector3.Distance(m_Transform.position, objectsInSight[nearbyEnemies[i]].LastKnownPosition) / SightRange;

                if (distPercentage > 1f)
                    continue;

                for (int k = 0; k < enemyTraitReactions.Count; k++)
                {
                    float traitDelta = enemyTraitReactions[k].sightCurve.Evaluate(distPercentage) * Time.deltaTime;

                    ChangeTrait(enemyTraitReactions[k].traitType, traitDelta, nearbyEnemies[i]);
                }
            }

            //Projectiles
            for (int i = 0; i < nearbyProjectiles.Count; i++)
            {
                float distPercentage = Vector3.Distance(m_Transform.position, nearbyProjectiles[i].Position) / SightRange;

                if (distPercentage > 1f)
                    continue;


                for (int k = 0; k < projectileTraitReactions.Count; k++)
                {
                    float traitDelta = projectileTraitReactions[k].sightCurve.Evaluate(distPercentage) * Time.deltaTime;

                    ChangeTrait(projectileTraitReactions[k].traitType, traitDelta, nearbyProjectiles[i].Owner);
                }
            }
                    
        }
    }




    IEnumerator ReactionDelay(Collider coll)
    {
        yield return new WaitForSeconds(reactionTime);

        CheckNewObject(coll);
    }
    IEnumerator ReactionDelay(GameObject obj)
    {
        yield return new WaitForSeconds(reactionTime);

        CheckNewObject(obj);
    }

    void CheckNewObject(Collider coll)
    {
        if (coll == null)
            return;



        GameObject obj = coll.gameObject;
        SightedObject sightedObj = null;// new SightedObject(obj.transform, obj.transform.position, Vector3.zero, false);
        

        bool hasSeenBefore = objectsInSight.ContainsKey(obj.transform);
        bool shouldAddToSight = false;

        if (!hasSeenBefore)
        {
            if(nearbyTags.ContainsKey(obj.tag)){
                List<Transform> tempList = nearbyTags[obj.tag];

                bool shouldAdd = true;
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i] == obj.transform)
                    {
                        shouldAdd = false;
                        break;
                    }
                }

                if (shouldAdd)
                    nearbyTags[obj.tag].Add(obj.transform);
            }
            else
            {
                nearbyTags.Add(obj.tag, new List<Transform>() { obj.transform });
            }


            sightedObj = new SightedObject(obj.transform, obj.transform.position, Vector3.zero, false);
        }


        if (IsAlly(coll))
        {
            if (!hasSeenBefore)
            {
                nearbyAllies.Add(sightedObj.SightedTransform);
            }

            shouldAddToSight = true;
        }

        if (IsEnemy(coll))
        {
            if (!hasSeenBefore)
            {
                nearbyEnemies.Add(sightedObj.SightedTransform);
            }

            if(TargetTransform == null && Utilities.HasFlag(initialTargetingMethod, TargetingMethod.OnSight))
            {
                CheckTarget(coll.transform);
            }
            else if(TargetTransform != null && Utilities.HasFlag(reTargetingMethod, TargetingMethod.OnSight))
            {
                CheckRetarget(coll.transform);
            }

            shouldAddToSight = true;
        }

        if (IsWeapon(coll))
        {
            if (!hasSeenBefore)
            {
                nearbyWeapons.Add(sightedObj.SightedTransform);
            }

            shouldAddToSight = true;
        }

        if (IsAbility(coll))
        {
            if (!hasSeenBefore)
            {
                nearbyAbilities.Add(sightedObj.SightedTransform);
            }

            shouldAddToSight = true;
        }

        if (IsHealth(coll))
        {
            if (!hasSeenBefore)
            {
                nearbyHealth.Add(sightedObj.SightedTransform);
            }

            shouldAddToSight = true;
        }

        if (IsExperience(coll))
        {
            if (!hasSeenBefore)
            {
                nearbyExperience.Add(sightedObj.SightedTransform);
            }

            shouldAddToSight = true;
        }

        if (IsProjectile(coll))
        {
            
            if (!hasSeenBefore)
            {
                IProjectile pScript = obj.GetComponent<IProjectile>();

                if(pScript != null)
                    nearbyProjectiles.Add(pScript);
            }

            shouldAddToSight = true;
        }

      

        if (forgetSet.Contains(obj))
        {
            StopCoroutine(ForgetObject(obj));
            forgetSet.Remove(obj);
        }


        if (shouldAddToSight && !hasSeenBefore)
            objectsInSight.Add(sightedObj.SightedTransform, sightedObj);

    }
    void CheckNewObject(GameObject obj)
    {
        if (obj == null)
            return;


        Collider[] colls = obj.GetComponents<Collider>();

        for(int i = 0; i < colls.Length; i++)
        {
            CheckNewObject(colls[i]);
        }
    }

    public List<Transform> GetNearbyObject(string tag)
    {
        return nearbyTags.ContainsKey(tag) ? nearbyTags[tag] : new List<Transform>();
    }





    public override void NoiseHeard(AudioClip noise, Transform noiseOwner, Vector3 noisePosition, float noiseVolume)
    {
        if (noiseVolume < HearingThreshold)
            return;
    }










    /*
    void OnTriggerStay(Collider coll)
    {
        GameObject obj = coll.gameObject;
        Vector3 toVector = obj.transform.position - myTransform.position;

        bool isInSight = true;

        //Can see transform?
        isInSight = Vector3.Angle(myTransform.forward, toVector) < (FOV / 2f);
        //isInSight = isInSight && !Physics.Raycast(new Ray(myTransform.position, toVector), toVector.magnitude + 0.1f, environmentMask);

        if (!canSeeThroughObjects)
        {
            RaycastHit[] _hits = Physics.RaycastAll(myTransform.position, toVector, toVector.magnitude);
            for (int i = 0; i < _hits.Length; i++)
            {
                if (_hits[i].collider.isTrigger)
                    continue;

                if (Vector3.Distance(_hits[i].point, myTransform.position) < toVector.magnitude && _hits[i].collider.gameObject != obj)
                    isInSight = false;
            }
        }

        //React if can see
        if (isInSight)
            StartCoroutine(ReactionDelay(coll));


        //Update last known info for object
        if (objectsInSight.ContainsKey(coll.transform))
        {
            SightedObject _sightedObj = objectsInSight[coll.transform];
            _sightedObj.sightedTransform = coll.transform;
            _sightedObj.inSight = isInSight;

            if (isInSight)
            {
                _sightedObj.lastKnownPosition = coll.transform.position;
                _sightedObj.lastTimeSeen = Time.time;

                Rigidbody _rigid = coll.transform.GetComponent<Rigidbody>();
                if (_rigid != null)
                    _sightedObj.lastKnownDirection = _rigid.velocity;
            }
        }

    }*/
    /*
    void OnTriggerExit(Collider coll)
    {
        GameObject obj = coll.gameObject;


        if (forgetSet.Contains(obj))
        {
            StopCoroutine(ForgetObject(obj));
            forgetSet.Remove(obj);
        }

        StartCoroutine(ForgetObject(obj));

        //if (sightDictionary.ContainsKey(obj))
        //    sightDictionary.Remove(obj);
    }
    */




    /// <summary>
    /// Check for nearby objects consistently
    /// </summary>
    IEnumerator CheckNearbyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkNearbyTime);

            CheckNearby();
        }
    }
    /// <summary>
    /// Perform actual check for nearby objects
    /// </summary>
    void CheckNearby()
    {
        Collider[] colls = Physics.OverlapSphere(m_Transform.position, SightRange);

        //Get set of objects within sight
        HashSet<GameObject> noticedSet = new HashSet<GameObject>();
        List<Transform> sightList = new List<Transform>(objectsInSight.Keys);
        for (int i = 0; i < sightList.Count; i++)
        {
            if (sightList[i] != null && sightList[i].gameObject.activeInHierarchy)
                noticedSet.Add(sightList[i].gameObject);
        }
        

        for (int i = 0; i < colls.Length; i++)
        {
            //Have we seen this object before?
            if (noticedSet.Contains(colls[i].gameObject))
                noticedSet.Remove(colls[i].gameObject);

            CheckNearbyObject(colls[i]);
        }


        //Start to forget objects that weren't encountered during OverlapSphere
        IEnumerator _enumerator = noticedSet.GetEnumerator();
        while (_enumerator.MoveNext())
        {
            GameObject g = _enumerator.Current as GameObject;

            if (g == null)
                continue;

            if (!forgetSet.Contains(g))
            {
                StartCoroutine(ForgetObject(g));
            }
        }
    }
    /// <summary>
    /// Check an object to see if it's in sight and/or needs to be forgotten
    /// </summary>
    void CheckNearbyObject(Collider coll)
    {
        GameObject obj = coll.gameObject;

        if (obj == this.gameObject)
            return;


        Vector3 toVector = obj.transform.position - m_Transform.position;

        bool isInSight = true;

        //Is transform within FOV?
        isInSight = Vector3.Angle(m_Transform.forward, toVector) < (FOV / 2f);

        if (!canSeeThroughObjects)
        {
            //Check if a non-trigger collider is blocking vision
            RaycastHit[] _hits = Physics.RaycastAll(m_Transform.position, toVector, toVector.magnitude);
            for (int i = 0; i < _hits.Length; i++)
            {
                if (_hits[i].collider.isTrigger)
                    continue;

                if (Vector3.Distance(_hits[i].point, m_Transform.position) < toVector.magnitude && _hits[i].collider.gameObject != obj)
                    isInSight = false;
            }
        }

        //React if can see the object
        if (isInSight)
            StartCoroutine(ReactionDelay(coll));


        //Update last known info for object
        if (objectsInSight.ContainsKey(coll.transform))
        {
            SightedObject _sightedObj = objectsInSight[coll.transform];
           // _sightedObj.SightedTransform = coll.transform;
            _sightedObj.InSight = isInSight;

            if (isInSight)
            {
                _sightedObj.LastKnownPosition = coll.transform.position;
                _sightedObj.LastTimeSeen = Time.time;

                Rigidbody _rigid = coll.transform.GetComponent<Rigidbody>();
                if (_rigid != null)
                    _sightedObj.LastKnownDirection = _rigid.velocity;
            }
        }

    }





    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();


        if (showDebug && m_Transform != null)
        {

            if (Path != null)
            {
                Gizmos.color = Color.white;
                for (int i = 0; i < Path.Count; i++)
                {
                    Gizmos.DrawCube(Path[i], Vector3.one * ((A_Star_Pathfinding.Instance.NodeRadius * 2) *(1 - A_Star_Pathfinding.NODE_BUFFER_PERCENTAGE)));
                }


                Gizmos.color = Color.red;
                for (int i = 0; i < Path.Count - 1; i++)
                {
                    Gizmos.DrawLine(Path[i], Path[i + 1]);
                }
            }

        }

    }


    public override void OnValidate()
    {
        base.OnValidate();

        checkNearbyTime = Mathf.Max(0.1f, checkNearbyTime);
        MemoryTime = MemoryTime;

        Utilities.ValidateCurve_Times(poorDecisionCurve, 0f, 100f);
        Utilities.ValidateCurve_Times(fovLevelUp, 0f, 1f);

        allyTraitReactions.ForEach(r => r.Validate());
        enemyTraitReactions.ForEach(r => r.Validate());
        projectileTraitReactions.ForEach(r => r.Validate());
    }
}



[System.Serializable]
public class SightedObject
{
    private Transform sightedTransform;
    private Vector3 lastKnownPosition;
    private Vector3 lastKnownDirection;
    private float lastTimeSeen;
    private bool inSight;

    public SightedObject()
    {

    }
    public SightedObject(Transform _transform, Vector3 _pos, Vector3 _dir, bool _sight)
    {
        sightedTransform = _transform;
        lastKnownPosition = _pos;
        lastKnownDirection = _dir;
        lastTimeSeen = Time.time;
        inSight = _sight;

    }


    public Transform SightedTransform
    {
        get { return sightedTransform; }
    }
    public Vector3 LastKnownPosition
    {
        get { return lastKnownPosition; }
        set { lastKnownPosition = value; }
    }
    public Vector3 LastKnownDirection
    {
        get { return lastKnownDirection; }
        set { lastKnownDirection = value; }
    }
    public float LastTimeSeen
    {
        get { return lastTimeSeen; }
        set { lastTimeSeen = value; }
    }
    public bool InSight
    {
        get { return inSight; }
        set { inSight = value; }
    }



    public override string ToString()
    {
        return string.Format("Sighted Transform: {0}. Last Known Position: {1}. Last Known Direction: {2}. Last Time Seen: {3}. In Sight: {4}", SightedTransform, LastKnownPosition, LastKnownDirection, LastTimeSeen, InSight);
    }
}
