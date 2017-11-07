using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class UtilityActor : UnitController
{
    static readonly float MINIMUM_UPDATE_TIME = 0.02f;



    List<BaseUtilityBehavior> m_Behaviors = new List<BaseUtilityBehavior>();

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

    List<IMemorable> nearbyAllies = new List<IMemorable>();
    List<IMemorable> nearbyEnemies = new List<IMemorable>();

    List<IMemorable> nearbyWeapons = new List<IMemorable>();
    List<IMemorable> nearbyAbilities = new List<IMemorable>();

    List<IMemorable> nearbyHealth = new List<IMemorable>();
    List<IMemorable> nearbyExperience = new List<IMemorable>();
    List<IProjectile> nearbyProjectiles = new List<IProjectile>();

    //Linking of gameobject Tag to nearby Transforms
    Dictionary<string, List<Transform>> nearbyTags = new Dictionary<string, List<Transform>>();

    //Track objects that are in sight
    Dictionary<Transform, SightedObject> objectsInSight = new Dictionary<Transform, SightedObject>();

    //Track objects that are being forgotten
    Dictionary<IMemorable, float> memoryTracker = new Dictionary<IMemorable, float>();
    //HashSet<GameObject> objectsToForget = new HashSet<GameObject>();
    HashSet<IMemorable> unforgetableObjects = new HashSet<IMemorable>();


    Transform targetTransform;
    Transform followTransform;



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

    [SerializeField]
    UtilityPsyche m_Psyche;


    [Serializable]
    struct TraitReaction
    {
        public UtilityPersonalityTrait.PersonalityTrait_Type traitType;
        public AnimationCurve sightCurve;
        public AnimationCurve hearingCurve;
        public AnimationCurve injuryCurve;




        public void Validate()
        {
            Utilities.ValidateCurve_Times(sightCurve, 0f, 1f);
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
    IPathfinder m_Pathfinder;


    public delegate void AlertEvent(Transform t);
    public AlertEvent OnDamage;
    public AlertEvent OnSight;
    public AlertEvent OnSound;


    public override void Awake()
    {
        base.Awake();

        m_Pathfinder = GetComponent<IPathfinder>();

        m_Health.OnDamaged += HealthChanged;
    }
    public override void Start()
    {
        base.Start();


        if (GameManager.Instance != null)
        {
            m_Health.OnKilled += GameManager.Instance.EnemyKilled;
        }


        //ScaleAI();

        m_Psyche.Initialize(m_Health);
    }
    public void OnEnable()
    {
        GatherAllBehaviors();


        StartCoroutine(UpdateBehavior());
        StartCoroutine(UpdateInfluencers());

        // StartCoroutine(CheckNearbyRoutine());
    }
    public override void OnDisable()
    {
        base.OnDisable();

        StopAllCoroutines();

        if (m_TextUI != null)
        {
            m_TextUI.enabled = false;
        }
        //DeflateUtilityUI();
    }

    void Update()
    {
        MemoryUpdate(Time.deltaTime);
        m_Psyche.Update(Time.deltaTime);
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
        if (m_Behaviors.Count == 0)
            return;

        if (currentActiveBehavior != null && !currentActiveBehavior.CanEndBehavior)
        {
            if (ShowDebug)
            {
                Debug.Log(m_Transform.name + " -- Can't end current behavior. Current Behavior: " + CurrentBehavior.ToString());
            }


            return;
        }

        List<WeightedObjectOfUtilityBehavior> weightedBehaviors = new List<WeightedObjectOfUtilityBehavior>();


        for (int i = 0; i < m_Behaviors.Count; i++)
        {
            if (!m_Behaviors[i].IsActive && !m_Behaviors[i].CanStartBehavior)
            {
                continue;
            }

            float _weight = m_Behaviors[i].GetBehaviorScore();

            if (_weight <= 0f)
            {
                continue;
            }

            weightedBehaviors.Add(new WeightedObjectOfUtilityBehavior(m_Behaviors[i], _weight));

        }


        if (weightedBehaviors.Count == 0)
        {
            if (ShowDebug)
            {
                Debug.Log(m_Transform.name + " -- No weighted behaviors available.");
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

            considerationBehaviors.Add(weightedBehaviors[i]);
        }


        if (considerationBehaviors.Count == 0)
            return;


        BaseUtilityBehavior chosenBehavior = Utilities.WeightedSelection<BaseUtilityBehavior>(considerationBehaviors.ToArray(), 0f);

        if (chosenBehavior == currentActiveBehavior)
        {
            if (ShowDebug)
            {
                Debug.Log(m_Transform.name + " Chosen behavior is already current active behavior: " + currentActiveBehavior.ToString());
            }

            return;
        }

        if (ShowDebug)
        {
            if (currentActiveBehavior != null)
            {
                Debug.Log(m_Transform.name + " -- Ending Behavior -- " + currentActiveBehavior.ToString());
            }


            float _score = -1f;

            for (int i = 0; i < considerationBehaviors.Count; i++)
            {
                if (considerationBehaviors[i].Item1 == chosenBehavior)
                {
                    _score = considerationBehaviors[i].Item2;
                }
            }

            Debug.Log(m_Transform.name + " -- New Behavior -- " + chosenBehavior.ToString() + " ## Score: " + _score);
        }





        if (currentActiveBehavior == null)
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
        m_Behaviors.Add(_behavior);
    }

    /// <summary>
    /// Remove behavior from list of behaviors to check
    /// </summary>
    public bool RemoveBehavior(BaseUtilityBehavior _behavior)
    {
        int index = -1;

        for (int i = 0; i < m_Behaviors.Count; i++)
        {
            if (m_Behaviors[i] == _behavior)
            {
                index = i;
                break;
            }
        }


        if (index == -1)
            return false;


        m_Behaviors.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Gather list of all behaviors to check
    /// </summary>
    public void GatherAllBehaviors()
    {
        m_Behaviors.Clear();

        BaseUtilityBehavior[] _behaviors = GetComponents<BaseUtilityBehavior>();

        for (int i = 0; i < _behaviors.Length; i++)
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
            Debug.Log(m_Transform.name + " -- Behavior Ended -- " + _behavior.ToString());
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
            yield return new WaitForSeconds(updateBehaviorTime);


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


    #region Stat Stuff


    /// <summary>
    /// Scale AI to Current Level
    /// </summary>
    //public void ScaleAI()
    //{
    //    if (GameManager.Instance == null)
    //        return;



    //    float bonusFOV = 0f;

    //    for (int i = 1; i < GameManager.Instance.CurrentLevel; i++)
    //    {
    //        if (UnityEngine.Random.value <= chanceHealthLevelUp)
    //        {
    //            ChangeStat(StatType.Health, 1);
    //        }

    //        if (UnityEngine.Random.value <= chanceSpeedLevelUp)
    //        {
    //            ChangeStat(StatType.Speed, 1);
    //        }

    //        if (UnityEngine.Random.value <= chanceDexterityLevelUp)
    //        {
    //            ChangeStat(StatType.Dexterity, 1);
    //        }

    //        if (UnityEngine.Random.value <= chanceDamageLevelUp)
    //        {
    //            ChangeStat(StatType.Damage, 1);
    //        }

    //        if (UnityEngine.Random.value <= chanceCriticalDamageLevelUp)
    //        {
    //            ChangeStat(StatType.CriticalDamage, 1);
    //        }

    //        if (UnityEngine.Random.value <= chanceLuckLevelUp)
    //        {
    //            ChangeStat(StatType.Luck, 1);
    //        }

    //        bonusFOV += fovLevelUp.Evaluate(UnityEngine.Random.value);
    //    }

    //    UpdateAllStatEffects();
    //    FOV += (int)bonusFOV;

    //}


    //protected override void UpdateStatEffects(StatType _type)
    //{
    //    base.UpdateStatEffects(_type);

    //    Stat _stat = m_Stats.GetStat(_type);

    //    if (_stat == null)
    //        return;


    //    switch (_stat.Type)
    //    {
    //        case StatType.Dexterity:
    //            BonusAttackRate = _stat.CurrentValue;
    //            BonusAttackSpeed = _stat.CurrentValue;
    //            break;
    //        case StatType.Damage:
    //            BonusAttackPower = (int)_stat.CurrentValue;
    //            break;
    //        case StatType.CriticalDamage:
    //            BonusCriticalHitMultiplier = _stat.CurrentValue;

    //            break;
    //        case StatType.Luck:
    //            BonusCriticalHitChance = _stat.CurrentValue;

    //            break;
    //        default:
    //            break;
    //    }
    //}

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
        m_Psyche.ModifyTrait(_type, _delta, _influencer);
    }

    /// <summary>
    /// React to Damage
    /// </summary>
    public override void HealthChanged(Health _health)
    {
        base.HealthChanged(_health);

        float pctg = Mathf.Clamp01(Mathf.Abs(_health.LastHealthChange / _health.MaxHealth));

        //Allies
        for (int i = 0; i < nearbyAllies.Count; i++)
        {
            if (!nearbyAllies[i].Transform == _health.LastAttacker)
                continue;


            for (int k = 0; k < allyTraitReactions.Count; k++)
            {
                float traitDelta = allyTraitReactions[k].injuryCurve.Evaluate(pctg);

                ChangeTrait(allyTraitReactions[i].traitType, traitDelta, nearbyAllies[i].Transform);
            }
        }

        //Enemies
        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (!nearbyEnemies[i].Transform == _health.LastAttacker)
                continue;


            for (int k = 0; k < enemyTraitReactions.Count; k++)
            {
                float traitDelta = enemyTraitReactions[k].injuryCurve.Evaluate(pctg);

                ChangeTrait(enemyTraitReactions[i].traitType, traitDelta, nearbyEnemies[i].Transform);
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
    public override void Died(Health _casualtyHealth)
    {
        base.Died(_casualtyHealth);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyKilled(m_Health);
        }
    }

    #endregion


    #region Target Selection

    public Transform TargetEnemy_Random()
    {
        List<Transform> validEnemies = new List<Transform>();


        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (!nearbyEnemies[i].GameObject.activeInHierarchy)
            {
                nearbyEnemies.RemoveAt(i);
                i--;
                continue;
            }

            if (!EnemyTraitThresholdCheck(nearbyEnemies[i].Transform))
                continue;

            validEnemies.Add(nearbyEnemies[i].Transform);
        }

        if (validEnemies.Count == 0)
            return null;


        int index = UnityEngine.Random.Range(0, validEnemies.Count);

        return nearbyEnemies[index].Transform;
    }
    //public Transform TargetEnemy_ShortestDistance()
    //{

    //    List<CustomTuple2<Transform, float>> validEnemies = new List<CustomTuple2<Transform, float>>();


    //    for (int i = 0; i < nearbyEnemies.Count; i++)
    //    {
    //        if (nearbyEnemies[i] == null || !nearbyEnemies[i].GameObject.activeInHierarchy)
    //        {
    //            nearbyEnemies.RemoveAt(i);
    //            i--;
    //            continue;
    //        }

    //        if (!EnemyTraitThresholdCheck(nearbyEnemies[i].Transform))
    //            continue;

    //        float dist = A_Star_Pathfinding.Instance.EstimatePathDistance(m_Transform.position, nearbyEnemies[i].Transform.position, Utilities.CalculateObjectBounds(gameObject, false), WalkableNodes);

    //        validEnemies.Add(new CustomTuple2<Transform, float>(nearbyEnemies[i].Transform, dist));

    //    }

    //    if (validEnemies.Count == 0)
    //        return null;


    //    validEnemies.Sort(delegate (CustomTuple2<Transform, float> x, CustomTuple2<Transform, float> y)
    //    {
    //        return x.Item2.CompareTo(y.Item2);
    //    });





    //    List<WeightedObject<Transform>> weightedDistances = new List<WeightedObject<Transform>>();

    //    int numToConsider = (int)(poorDecisionCurve.Evaluate(stupidityLevel));
    //    numToConsider = numToConsider == 0 ? 1 : numToConsider;

    //    for (int i = 0; i < numToConsider; i++)
    //    {
    //        weightedDistances.Add(new WeightedObject<Transform>(validEnemies[i].Item1, validEnemies[i].Item2));
    //    }

    //    return Utilities.WeightedSelection<Transform>(weightedDistances.ToArray(), 0f);
    //}
    public Transform TargetEnemy_MostHealth()
    {

        List<CustomTuple2<Transform, float>> validEnemies = new List<CustomTuple2<Transform, float>>();


        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            if (nearbyEnemies[i] == null || !nearbyEnemies[i].GameObject.activeInHierarchy)
            {
                nearbyEnemies.RemoveAt(i);
                i--;
                continue;
            }

            if (!EnemyTraitThresholdCheck(nearbyEnemies[i].Transform))
                continue;

            Health _health = nearbyEnemies[i].GameObject.GetComponent<Health>();

            if (_health == null)
                continue;

            validEnemies.Add(new CustomTuple2<Transform, float>(nearbyEnemies[i].Transform, _health.CurHealth));

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
            if (nearbyEnemies[i] == null || !nearbyEnemies[i].GameObject.activeInHierarchy)
            {
                nearbyEnemies.RemoveAt(i);
                i--;
                continue;
            }

            if (!EnemyTraitThresholdCheck(nearbyEnemies[i].Transform))
                continue;

            Health _health = nearbyEnemies[i].GameObject.GetComponent<Health>();

            if (_health == null)
                continue;

            validEnemies.Add(new CustomTuple2<Transform, float>(nearbyEnemies[i].Transform, _health.CurHealth));

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


        if (Utilities.HasFlag(_method, TargetingMethod.LeastHealth))
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

        //if (Utilities.HasFlag(_method, TargetingMethod.ShortestDistance))
        //{
        //    t = TargetEnemy_ShortestDistance();

        //    if (t != null)
        //    {
        //        possibleTargets.Add(t);
        //    }
        //}


        if (possibleTargets.Count == 0)
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
            UtilityPersonalityTrait _trait = m_Psyche.GetTrait(enemyTraitThresholds[i].traitType);

            if (_trait == null)
                continue;

            if (_trait.GetInfluenceAmount(_enemy) < enemyTraitThresholds[i].threshold)
                isValid = false;
        }

        return isValid;
    }

    #endregion


    #region Entity Memory

    void MemoryUpdate(float deltaTime)
    {
        Dictionary<IMemorable, float> newMemoryTracker = new Dictionary<IMemorable, float>();

        Dictionary<IMemorable, float>.Enumerator memoryEnumerator = memoryTracker.GetEnumerator();
        while (memoryEnumerator.MoveNext())
        {
            GameObject g;

            //Ignore deleted or inactive objects
            if (memoryEnumerator.Current.Key != null && (g = memoryEnumerator.Current.Key.GameObject).activeInHierarchy)
            {
                float t = memoryEnumerator.Current.Value - deltaTime;

                //Keep memory of object if memory time remains OR object is unforgettable
                if (t > 0 || unforgetableObjects.Contains(memoryEnumerator.Current.Key))
                {
                    newMemoryTracker.Add(memoryEnumerator.Current.Key, t);
                }
                else
                {
                    Debug.Log(m_Transform + " --FORGETTING---> " + memoryEnumerator.Current.Key.GameObject);
                }
            }
        }

        memoryTracker = newMemoryTracker;

        CheckMemoryLists();
    }
    void CheckMemoryLists()
    {
        nearbyAllies.Where(a => memoryTracker.ContainsKey(a));
        nearbyEnemies.Where(a => memoryTracker.ContainsKey(a));
        nearbyExperience.Where(a => memoryTracker.ContainsKey(a));
        nearbyHealth.Where(a => memoryTracker.ContainsKey(a));
        nearbyWeapons.Where(a => memoryTracker.ContainsKey(a));
        nearbyAbilities.Where(a => memoryTracker.ContainsKey(a));

        nearbyProjectiles.Where(a => memoryTracker.ContainsKey(a.GameObject.GetComponent<IMemorable>()));

        objectsInSight.Where(a => memoryTracker.ContainsKey(a.Key.GetComponent<IMemorable>())).ToDictionary(a => a.Key, a => a.Value);

        if (TargetTransform != null && !memoryTracker.ContainsKey(TargetTransform.GetComponent<IMemorable>()))
        {
            TargetTransform = null;
        }
    }


    public bool IsAlly(Team teamMember)
    {
        if (teamMember == null)
            return false;

        return m_Team.IsFriendly(teamMember);
        //return Utilities.IsInLayerMask(obj.gameObject, friendlyTeam) && allyTags.Contains(obj.gameObject.tag);
    }
    public bool IsEnemy(Team teamMember)
    {
        if (teamMember == null)
            return false;

        bool val = m_Team.IsEnemy(teamMember);

        return val;
        //return Utilities.IsInLayerMask(obj.gameObject, enemyTeam) && !allyTags.Contains(obj.gameObject.tag);
    }

    public bool IsWeapon(GameObject obj)
    {
        return obj.GetComponent<Weapon>() != null;
    }
    public bool IsAbility(GameObject obj)
    {
        return obj.GetComponent<Ability>() != null;
    }

    public bool IsHealth(GameObject obj)
    {
        return obj.GetComponent<IHealthProvider>() != null;
    }
    public bool IsExperience(GameObject obj)
    {
        return obj.GetComponent<IExperienceProvider>() != null;
    }
    public bool IsProjectile(GameObject obj)
    {
        return obj.GetComponent<IProjectile>() != null;
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
            m_TextUI.Initialize(m_Transform);


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





    #region Accessors

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


    public float MemoryTime
    {
        get { return memoryTime; }
        private set
        {
            memoryTime = value;

            if (memoryTime < -1)
            {
                memoryTime = -1;
            }
        }
    }
    public float ReactionTime
    {
        get { return reactionTime; }
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

                    if (t != null)
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

            //if (TargetTransform == null)
            //{
            //    Transform t = ChooseTarget(initialTargetingMethod);

            //    if (t != null)
            //    {
            //        TargetTransform = t;
            //    }
            //}

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
            if (value == null)
            {
                Debug.Log("Setting TargetTransform to NULL");
            }
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


    public List<IMemorable> NearbyAllies
    {
        get
        {
            for (int i = 0; i < nearbyAllies.Count; i++)
            {
                if (nearbyAllies[i] == null || !nearbyAllies[i].GameObject.activeInHierarchy)
                {
                    nearbyAllies.RemoveAt(i);
                    i--;
                }
            }
            return nearbyAllies;
        }
    }
    public List<IMemorable> NearbyEnemies
    {
        get
        {
            for (int i = 0; i < nearbyEnemies.Count; i++)
            {
                if (nearbyEnemies[i] == null || !nearbyEnemies[i].GameObject.activeInHierarchy)
                {
                    nearbyEnemies.RemoveAt(i);
                    i--;
                }
            }

            return nearbyEnemies;
        }
    }
    public List<IMemorable> NearbyWeapons
    {
        get
        {

            for (int i = 0; i < nearbyWeapons.Count; i++)
            {
                if (nearbyWeapons[i] == null || !nearbyWeapons[i].GameObject.activeInHierarchy)
                {
                    nearbyWeapons.RemoveAt(i);
                    i--;
                }
            }
            return nearbyWeapons;
        }
    }
    public List<IMemorable> NearbyAbilities
    {
        get
        {

            for (int i = 0; i < nearbyAbilities.Count; i++)
            {
                if (nearbyAbilities[i] == null || !nearbyAbilities[i].GameObject.activeInHierarchy)
                {
                    nearbyAbilities.RemoveAt(i);
                    i--;
                }
            }

            return nearbyAbilities;
        }
    }
    public List<IMemorable> NearbyHealth
    {
        get
        {
            for (int i = 0; i < nearbyHealth.Count; i++)
            {
                if (nearbyHealth[i] == null || !nearbyHealth[i].GameObject.activeInHierarchy)
                {
                    nearbyHealth.RemoveAt(i);
                    i--;
                }
            }
            return nearbyHealth;
        }
    }
    public List<IMemorable> NearbyExperience
    {
        get
        {
            for (int i = 0; i < nearbyExperience.Count; i++)
            {
                if (nearbyExperience[i] == null || !nearbyExperience[i].GameObject.activeInHierarchy)
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



    IEnumerator UpdateInfluencers()
    {
        while (true)
        {
            yield return null;


            //Allies
            for (int i = 0; i < nearbyAllies.Count; i++)
            {

                float distPercentage = Vector3.Distance(m_Transform.position, objectsInSight[nearbyAllies[i].Transform].LastKnownBasePosition) / SightRange;

                if (distPercentage > 1f)
                    continue;

                for (int k = 0; k < allyTraitReactions.Count; k++)
                {
                    float traitDelta = allyTraitReactions[k].sightCurve.Evaluate(distPercentage) * Time.deltaTime;

                    ChangeTrait(allyTraitReactions[k].traitType, traitDelta, nearbyAllies[i].Transform);
                }
            }

            //Enemies
            for (int i = 0; i < nearbyEnemies.Count; i++)
            {
                float distPercentage = Vector3.Distance(m_Transform.position, objectsInSight[nearbyEnemies[i].Transform].LastKnownBasePosition) / SightRange;

                if (distPercentage > 1f)
                    continue;

                for (int k = 0; k < enemyTraitReactions.Count; k++)
                {
                    float traitDelta = enemyTraitReactions[k].sightCurve.Evaluate(distPercentage) * Time.deltaTime;

                    ChangeTrait(enemyTraitReactions[k].traitType, traitDelta, nearbyEnemies[i].Transform);
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



    IEnumerator ReactionDelay(IMemorable memObj)
    {
        yield return new WaitForSeconds(reactionTime);

        AlertObject(memObj, true);
    }

    public void AlertObjects(List<GameObject> objects, bool areForgetable)
    {
        if (objects == null)
        {
            return;
        }

        objects.ForEach(o => AlertObject(o.GetComponent<IMemorable>(), areForgetable));
    }
    public void AlertObjects(List<IMemorable> objects, bool areForgetable)
    {
        objects.ForEach(o => AlertObject(o, areForgetable));
    }
    public void AlertObject(IMemorable memObj)
    {
        AlertObject(memObj, true);
    }
    public void AlertObject(IMemorable memObj, bool isForgetable)
    {
        if (memObj == null)
            return;

        GameObject obj = memObj.GameObject;
        SightedObject sightedObj = null;// new SightedObject(obj.transform, obj.transform.position, Vector3.zero, false);

        bool hasSeenBefore = objectsInSight.ContainsKey(obj.transform);
        bool shouldAddToSight = false;

        if (!hasSeenBefore)
        {
            if (nearbyTags.ContainsKey(obj.tag))
            {
                List<Transform> tempList = nearbyTags[obj.tag];

                bool shouldAdd = true;
                for (int i = 0; i < tempList.Count; i++)
                {
                    if (tempList[i] == obj.transform)
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

            sightedObj = new SightedObject(obj.transform);
            sightedObj.UpdatePositions();
            sightedObj.InSight = false;
        }


        if (IsAlly(obj.GetComponent<Team>()))
        {
            if (!hasSeenBefore)
            {
                nearbyAllies.Add(memObj);
            }

            shouldAddToSight = true;
        }

        if (IsEnemy(obj.GetComponent<Team>()))
        {
            if (!hasSeenBefore)
            {
                nearbyEnemies.Add(memObj);
            }

            if (TargetTransform == null && Utilities.HasFlag(initialTargetingMethod, TargetingMethod.OnSight))
            {
                CheckTarget(obj.transform);
            }
            else if (TargetTransform != null && Utilities.HasFlag(reTargetingMethod, TargetingMethod.OnSight))
            {
                CheckRetarget(obj.transform);
            }

            shouldAddToSight = true;
        }

        if (IsWeapon(obj))
        {
            if (!hasSeenBefore)
            {
                nearbyWeapons.Add(memObj);
            }

            shouldAddToSight = true;
        }

        if (IsAbility(obj))
        {
            if (!hasSeenBefore)
            {
                nearbyAbilities.Add(memObj);
            }

            shouldAddToSight = true;
        }

        if (IsHealth(obj))
        {
            if (!hasSeenBefore)
            {
                nearbyHealth.Add(memObj);
            }

            shouldAddToSight = true;
        }

        if (IsExperience(obj))
        {
            if (!hasSeenBefore)
            {
                nearbyExperience.Add(memObj);
            }

            shouldAddToSight = true;
        }

        if (IsProjectile(obj))
        {

            if (!hasSeenBefore)
            {
                IProjectile pScript = obj.GetComponent<IProjectile>();

                if (pScript != null)
                    nearbyProjectiles.Add(pScript);
            }

            shouldAddToSight = true;
        }


        if (memoryTracker.ContainsKey(memObj))
        {
            memoryTracker[memObj] = MemoryTime;
        }
        else
        {
            memoryTracker.Add(memObj, MemoryTime);
        }

        if (!isForgetable && !unforgetableObjects.Contains(memObj))
        {
            unforgetableObjects.Add(memObj);
        }

        if (shouldAddToSight && !hasSeenBefore)
            objectsInSight.Add(sightedObj.SightedTransform, sightedObj);

    }


    public List<Transform> GetNearbyObject(string tag)
    {
        return nearbyTags.ContainsKey(tag) ? nearbyTags[tag] : new List<Transform>();
    }



    ///// <summary>
    ///// Check for nearby objects consistently
    ///// </summary>
    //IEnumerator CheckNearbyRoutine()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(checkNearbyTime);

    //        CheckNearby();
    //    }
    //}
    ///// <summary>
    ///// Perform actual check for nearby objects
    ///// </summary>
    //void CheckNearby()
    //{
    //    Collider[] nearbyColliders = Physics.OverlapSphere(m_Transform.position, SightRange);

    //    //Get set of objects within sight
    //    HashSet<GameObject> noticedSet = new HashSet<GameObject>();

    //    List<Transform> sightList = new List<Transform>(objectsInSight.Keys);
    //    sightList.ForEach(o =>
    //    {
    //        if (o != null && o.gameObject.activeInHierarchy)
    //        {
    //            noticedSet.Add(o.gameObject);
    //        }
    //    });



    //    for (int i = 0; i < nearbyColliders.Length; i++)
    //    {
    //        IMemorable memObj = nearbyColliders[i].GetComponent<IMemorable>();

    //        if (memObj == null)
    //            continue;

    //        //Have we seen this object before?
    //        if (noticedSet.Contains(nearbyColliders[i].gameObject))
    //            noticedSet.Remove(nearbyColliders[i].gameObject);

    //        CheckNearbyObject(memObj);
    //    }
    //}
    ///// <summary>
    ///// Check an object to see if it's in sight and/or needs to be forgotten
    ///// </summary>
    //void CheckNearbyObject(IMemorable mem)
    //{
    //    GameObject memObj = mem.GameObject;

    //    Vector3 toVector = memObj.transform.position - m_Transform.position;

    //    bool isInSight = true;

    //    //Is transform within FOV?
    //    isInSight = Vector3.Angle(m_Transform.forward, toVector) < (FOV / 2f);

    //    if (!canSeeThroughObjects)
    //    {
    //        //Check if a non-trigger collider is blocking vision
    //        RaycastHit[] _hits = Physics.RaycastAll(m_Transform.position, toVector, toVector.magnitude);
    //        for (int i = 0; i < _hits.Length; i++)
    //        {
    //            if (_hits[i].collider.isTrigger)
    //                continue;

    //            if (Vector3.Distance(_hits[i].point, m_Transform.position) < toVector.magnitude && _hits[i].collider.gameObject != memObj)
    //                isInSight = false;
    //        }
    //    }

    //    //React if can see the object
    //    if (isInSight)
    //    {
    //        StartCoroutine(ReactionDelay(mem));
    //    }

    //    //Update last known info for object
    //    if (objectsInSight.ContainsKey(mem.Transform))
    //    {
    //        SightedObject _sightedObj = objectsInSight[memObj.transform];
    //        // _sightedObj.SightedTransform = coll.transform;
    //        _sightedObj.InSight = isInSight;

    //        if (isInSight)
    //        {
    //            _sightedObj.LastKnownBasePosition = memObj.transform.position;

    //            List<LOS_Target> losTargets = new List<LOS_Target>(memObj.GetComponentsInChildren<LOS_Target>());
    //            List<Vector3> losPositions = new List<Vector3>();
    //            losTargets.ForEach(t => losPositions.Add(t.transform.position));

    //            _sightedObj.LastKnownPositions = losPositions;

    //            _sightedObj.LastTimeSeen = Time.time;

    //            Rigidbody _rigid = memObj.GetComponent<Rigidbody>();
    //            if (_rigid != null)
    //                _sightedObj.LastKnownDirection = _rigid.velocity;
    //        }
    //    }

    //}

    protected override void SightGained(GameObject obj)
    {
        if (obj == null)
            return;

        IMemorable memObj = obj.GetComponent<IMemorable>();

        if (memObj == null)
            return;

        StartCoroutine(ReactionDelay(memObj));
    }
    protected override void SightMaintained(GameObject obj)
    {
        if (obj == null)
            return;

        IMemorable memObj = obj.GetComponent<IMemorable>();

        if (memObj == null)
            return;

        if (!objectsInSight.ContainsKey(memObj.Transform))
            return;

        objectsInSight[memObj.Transform].UpdatePositions();
    }
    protected override void SightLost(GameObject obj)
    {
        if (obj == null)
            return;

        IMemorable memObj = obj.GetComponent<IMemorable>();

        if (memObj == null)
            return;

        if (!objectsInSight.ContainsKey(memObj.Transform))
            return;

        SightedObject _sightedObj = objectsInSight[memObj.Transform];
        _sightedObj.InSight = false;
    }



    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();


        if (showDebug && m_Transform != null)
        {
            DrawAllyGizmo();
            DrawEnemyGizmo();
        }

    }

    void DrawAllyGizmo()
    {
        Gizmos.color = Color.cyan;

        NearbyAllies.ForEach(a =>
        {
            Gizmos.DrawLine(m_Transform.position, a.Transform.position);
        });
    }
    void DrawEnemyGizmo()
    {
        Gizmos.color = Color.red;

        NearbyEnemies.ForEach(a =>
        {
            Gizmos.DrawLine(m_Transform.position, a.Transform.position);
        });
    }


    public override void OnValidate()
    {
        base.OnValidate();

        MemoryTime = MemoryTime;

        Utilities.ValidateCurve_Times(poorDecisionCurve, 0f, 100f);
        Utilities.ValidateCurve_Times(fovLevelUp, 0f, 1f);

        allyTraitReactions.ForEach(r => r.Validate());
        enemyTraitReactions.ForEach(r => r.Validate());
        projectileTraitReactions.ForEach(r => r.Validate());
    }
}