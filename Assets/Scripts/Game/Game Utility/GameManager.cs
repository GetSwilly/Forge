using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

public class GameManager : MonoBehaviour
{

    #region Static Variables

    //  static readonly string LOOT_DATABASE_PATH = "Load/LootDatabase";
    // static readonly string IN_GAME_OBJECT_FOLDER_PATH = "In-Game Objects";


    static readonly int START_LIVES_PLAYER = 1;
    static readonly int START_LIVES_WARD = 1;

    /*
    static readonly int SCORE_ONKILL = 100;
    static readonly int SCORE_ONPLAYERDAMAGED = -250;
    static readonly int SCORE_ONPLAYERKILLED = -750;
    static readonly int SCORE_ONWARDDAMAGED = -500;
    static readonly int SCORE_ONWARDKILLED = -1500;
    */
    static readonly float DEITY_FAVOR_MAX = 100f;

    static readonly float ENEMY_DEATH_WEIGHT = 1f;

    //static readonly float OBJECT_SPAWN_DIST = 3f;

    static readonly int DIV_HEALTH_VALUE = 25;
    static readonly float LAUNCH_POWER = 30f;
    static readonly float DROP_DELAY = 0.25f;


    static readonly Vector3 HUB_WORLD_PLAYER_SPAWN = new Vector3(0f, .5f, 0f);
    static readonly float KILL_DEPTH = -150f;
    static readonly float KILL_PLANE_SCALE = 150;

    #endregion

    

    [SerializeField]
    bool showDebug;



    public enum GameState { START, PLAYING, WON, LOST };
    GameState currentGameState;

    [SerializeField]
    int maxLevel = 5;
    int currentLevel = 0;

    [SerializeField]
    int lives_Player = 0;
    int lives_Ward = 0;


    [SerializeField]
    int currentLevelPoints = 0;


    float currentDeityFavor = 0;



    [SerializeField]
    AnimationCurve disasterWaitCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [SerializeField]
    AnimationCurve disasterFavorCurve = AnimationCurve.Linear(-1f, 0f, 1f, 1f);

    [SerializeField]
    AnimationCurve disasterLevelCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);



    int numKilled = 0;
    bool canStartDisaster = false;



    //Active objects on screen	
    GameObject player;

    PlayerController pController;
    UserInput pInput;



    [SerializeField]
    List<ItemPoolListDefinition> m_ItemPoolDefinitions = new List<ItemPoolListDefinition>();


    Dictionary<string, GameObject> lootDictionary = new Dictionary<string, GameObject>();

    List<string> encounteredItems = new List<string>();


    [SerializeField]
    BalancingProbabilities m_ItemClassProbabilities = new BalancingProbabilities();


    [SerializeField]
    GameObject StartGameWorld;

    [SerializeField]
    GameObject MidLevelWorld;

    [SerializeField]
    [Range(0f, 10f)]
    float m_TimeScale = 1f;
    
    FrameRateTracker frameTracker;
    Settings m_Settings;


    public delegate void AlertEvent();
    public event AlertEvent OnFavorChange;


    [HideInInspector]
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            Destroy(this);

        Instance = this;

        Application.targetFrameRate = -1;
    }
    void Start()
    {
        currentGameState = GameState.START;
        frameTracker = new FrameRateTracker();

        GameObject killPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        killPlane.name = "Kill Plane";

        killPlane.GetComponent<MeshRenderer>().enabled = false;
        killPlane.transform.localScale = Vector3.one * KILL_PLANE_SCALE;
        killPlane.transform.position = new Vector3(0f, KILL_DEPTH, 0f);
        killPlane.AddComponent<KillBox>();


        GameObject g = new GameObject("Generated Objects");
        g.transform.position = transform.position;

        //LoadLootTables();

        UIManager.Instance.ShowTitleText("FORGE");
        
    }



    public void StartGame(PlayerController _player)
    {
        SetPlayer(_player);

        StartGame();
    }
    void StartGame()
    {
        currentLevel = 1;

        lives_Player = START_LIVES_PLAYER;
        lives_Ward = START_LIVES_WARD;



        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null && player != null)
        {
            camFollow.TargetTransform = player.transform;
        }


        LevelController.Instance.DestroyAllGenerated();
        //killDropLootTable.Initialize();
        //weaponLootTable.Initialize();
        //abilityLootTable.Initialize();

        currentGameState = GameState.PLAYING;


        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetColors();
            UIManager.Instance.InflateInGame();
        }



        StartCoroutine(NewLevel());
    }

    void EndGame()
    {

        pInput.CanAttack = false;
        pController.enabled = false;

        if (CurrentGameState == GameState.WON)
        {

        }
        else if (CurrentGameState == GameState.LOST)
        {

        }

        LevelController.Instance.DestroyAllGenerated();
        if (StartGameWorld != null)
        {
            StartGameWorld.SetActive(true);
        }


        currentLevel = 0;

        player.transform.position = Vector3.zero;
        pController.enabled = true;
        pInput.enabled = true;
        pInput.CanAttack = false;
    }

    public void NextLevel()
    {
        if (currentLevel == 0)
        {
            StartGame();
        }
        else
        {
            StartCoroutine(NewLevel());
        }
    }

    IEnumerator NewLevel()
    {

        UIManager.Instance.DeflateAll();

        if (StartGameWorld != null)
        {
            StartGameWorld.SetActive(false);
        }

        if (MidLevelWorld != null)
        {
            MidLevelWorld.SetActive(false);
        }

        List<UtilityActor> playerFollowers = pController.Followers;
        for (int i = 0; i < playerFollowers.Count; i++)
        {
            playerFollowers[i].gameObject.SetActive(false);
        }

        player.SetActive(false);

        numKilled = 0;

        StopCoroutine(DisasterTimerRoutine());
        canStartDisaster = false;

        yield return StartCoroutine(LevelController.Instance.GenerateLevel());

        SpawnPlayer();
        StartCoroutine(DisasterTimerRoutine());

        currentGameState = GameState.PLAYING;

        UIManager.Instance.DeflateAll();

        UIManager.Instance.InflateInGame();

        //UIManager.Instance.SetPause(false);

        //StartCoroutine(CheckForPause());

        LevelStartCamera c = Camera.main.GetComponent<LevelStartCamera>();
        if (c != null)
        {
            c.Initiate();
        }

    }

    public void EndLevel()
    {
        StopAllCoroutines();

        pController.enabled = false;
        pController.GetComponent<UserInput>().enabled = false;

        currentLevel++;

        if (currentLevel > maxLevel)
        {
            currentGameState = GameState.WON;
            EndGame();
        }
        else
        {

            LevelController.Instance.DestroyAllGenerated();
            ResetMidLevelWorld();

            //ShowUpgradeScreen(true);

            //CheckPlayerLevelUp();

            //StartCoroutine(WaitForUpgradeScreen());
            //NewLevel();
        }
    }

    void ResetMidLevelWorld()
    {
        if (MidLevelWorld != null)
        {
            MidLevelWorld.gameObject.SetActive(true);

            player.transform.position = MidLevelWorld.transform.TransformPoint(HUB_WORLD_PLAYER_SPAWN);
            pController.enabled = true;
            pController.GetComponent<UserInput>().enabled = true;
            pController.GetComponent<UserInput>().CanAttack = false;

            Camera.main.transform.position = MidLevelWorld.transform.TransformPoint(HUB_WORLD_PLAYER_SPAWN);
        }
    }

    IEnumerator DisasterTimerRoutine()
    {
        canStartDisaster = false;
        yield return new WaitForSeconds(disasterWaitCurve.Evaluate(UnityEngine.Random.value) + disasterFavorCurve.Evaluate(DeityFavor) + disasterLevelCurve.Evaluate(CurrentLevel));
        canStartDisaster = true;
    }
    public void StartDisaster(Disaster _disaster)
    {
        if (!CanStartDisaster)
            return;


        StartCoroutine(DisasterTimerRoutine());
    }


    void ShowUpgradeScreen(bool isEndOfLevel)
    {

        //Time.timeScale = 0f;

        pController.enabled = false;

        //CheckPlayerLevelUp();


        // StartCoroutine(WaitForUpgradeScreen(isEndOfLevel));
    }
   
    //IEnumerator CheckForPause()
    //{
    //    while (CurrentGameState == GameState.PLAYING)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Escape))
    //        {
    //            TogglePause();
    //        }

    //        yield return null;
    //    }
    //}

    /*
    //TODO --- Remove Coroutine and split into functions
    IEnumerator WaitForUpgradeScreen(bool isEndOfLevel)
    {

        UIManager.Instance.InflateUpgradeMenu();

        yield return null;

       // yield return StartCoroutine(UIManager.Instance.WaitForUpgradeMenu());

     

        if (isEndOfLevel)
            StartCoroutine(NewLevel());
    }
    */

    void SetPlayer(PlayerController _player)
    {
        if (_player == null)
            return;


        pController = _player;
        player = pController.gameObject;
        pInput = _player.GetComponent<UserInput>();
    }
    void SpawnPlayer()
    {

        if (player == null)
            return;

        Vector3 spawnPos = LevelController.Instance.StartGoalPosition;
        spawnPos.y += Utilities.CalculateObjectBounds(player, false).y / 2f;
        spawnPos.y += 0.1f;

        player.transform.position = spawnPos;
        player.transform.rotation = Quaternion.identity;

        player.SetActive(true);
        pInput.CanAttack = true;

        Rigidbody rigid = player.GetComponent<Rigidbody>();

        if (rigid != null)
        {
            rigid.velocity = Vector3.zero;
        }

        pController.enabled = true;
        pInput.CanAttack = true;


        List<UtilityActor> playerFollowers = pController.Followers;
        Rigidbody followerRigidbody;

        for (int i = 0; i < playerFollowers.Count; i++)
        {
            playerFollowers[i].gameObject.SetActive(true);
            followerRigidbody = playerFollowers[i].GetComponent<Rigidbody>();
            followerRigidbody.velocity = Vector3.zero;
        }


        //Health pHealth = player.GetComponent<Health>();
        //pHealth.ReviveMax();


        AttributeHandler pHandler = player.GetComponent<AttributeHandler>();
        if(pHandler != null)
        {
            pHandler.RemoveAllActiveAttributes();
        }
    }



    public void ResetLevel()
    {       //  ChangeScore(-Score);
        currentGameState = GameState.PLAYING;

        SpawnPlayer();
        //SpawnWard();

        //PauseMenu.SetActive(false);
        //Time.timeScale = 1.0f;

        UIManager.Instance.SetPause(false);
    }
    void GameOver()
    {
        currentGameState = GameState.LOST;
    }


    public void UnitDamaged(Health unitHealth)
    {
        GameObject unitObj = unitHealth.gameObject;

        switch (unitObj.tag)
        {
            case "Player":
                PlayerDamaged(unitObj);
                break;
            case "Ward":
                WardDamaged(unitObj);
                break;
            default:
                break;
        }

        Transform attackerTransform = unitHealth.LastAttacker;

        if (attackerTransform != null)
        {
            UnitController attackerController = attackerTransform.GetComponent<UnitController>();

            if (attackerController != null)
            {
                ///attackerController.DamageAchieved(unitHealth);
            }
        }
    }
    public void UnitKilled(Health unitHealth)
    {
        GameObject unitObj = unitHealth.gameObject;

        switch (unitObj.tag)
        {
            case "Player":
                PlayerKilled(unitObj);
                break;
            case "Ward":
                WardKilled(unitObj);
                break;
            default:
                numKilled++;
                EnemyKilled(unitObj);
                break;
        }

        unitObj.SetActive(false);


        /*
		Transform attackerTransform = unitHealth.LastAttacker;
		
		if(attackerTransform != null){
			MovementController attackerController = attackerTransform.GetComponent<MovementController>();
			
			if(attackerController != null){
				//attackerController.KillAchieved(unitHealth);
			}
		} */
    }


    public void PlayerDamaged(Health playerHealth)
    {
        PlayerDamaged(playerHealth.gameObject);
    }
    public void PlayerDamaged(GameObject playerObj)
    {
        //  ChangeScore(SCORE_ONPLAYERDAMAGED);
        CameraShake.Instance.ShakeMinor();
    }


    public void PlayerKilled(Health playerHealth)
    {
        PlayerKilled(playerHealth.gameObject);
    }
    public void PlayerKilled(GameObject playerObj)
    {
        lives_Player--;
        // ChangeScore(SCORE_ONPLAYERKILLED);

        if (lives_Player <= 0)
        {
            GameOver();
        }
        else
        {

            if (pController != null)
            {
                pController.ResetExp();
            }

            SpawnPlayer();
        }
    }


    void WardDamaged(GameObject wardObj)
    {
        // ChangeScore(SCORE_ONWARDDAMAGED);
        CameraShake.Instance.ShakeMajor();
    }
    void WardKilled(GameObject wardObj)
    {
        lives_Ward--;
        //ChangeScore(SCORE_ONWARDKILLED);

        if (lives_Ward <= 0)
        {
            GameOver();
        }
        else
        {
            //SpawnWard();
        }
    }


    public void EnemyKilled(Health enemyHealth)
    {
        EnemyKilled(enemyHealth.gameObject);
    }
    public void EnemyKilled(GameObject enemyObj)
    {
        if (CurrentGameState != GameState.PLAYING)
            return;


        StartCoroutine(EnemyKilled_DropRoutine(enemyObj.GetComponent<Health>()));

        A_Star_Pathfinding.Instance.AddWeightToNode(enemyObj.transform.position, ENEMY_DEATH_WEIGHT);
        enemyObj.SetActive(false);

        //  ChangeScore(SCORE_ONKILL);

    }
    IEnumerator EnemyKilled_DropRoutine(Health enemyHealth)
    {
        if (enemyHealth != null && enemyHealth.LastAttacker != null && enemyHealth.LastAttacker.tag == "Player")
        {
            int numObjs = enemyHealth.MaxHealth / DIV_HEALTH_VALUE;

            for (int i = 0; i < numObjs; i++)
            {
                DropLoot(enemyHealth.gameObject);

                yield return new WaitForSeconds(DROP_DELAY);
            }
        }
    }


    public bool CanModifyLevelPoints(int delta)
    {
        return LevelPoints + delta >= 0;
    }
    public void StoreLevelPoint()
    {

        int newLevels = pController.GetLevelPoints(1);


        if (newLevels > 0)
            AddLevelPoints(newLevels);

    }
    public void AddLevelPoints(int delta)
    {

        if (currentLevelPoints + delta < 0)
        {
            UnityEngine.Debug.Log("ERROR -- GameManager -- AddLevelPoints() - Adding delta would resuly in negative LevelPoints available.");
            return;
        }


        currentLevelPoints += delta;
    }

    //public GameObject GetLoot(RewardType _rewardType)
    //{
    //    return GetLoot(_rewardType, 0f);
    //}
    //public GameObject GetLoot(RewardType _rewardType, float _luckBonus)
    //{
    //    if (Utilities.HasFlag(_rewardType, RewardType.KillDrop))
    //    {
    //       return GetLoot_KillDrop(_luckBonus);
    //    }
    //    else if (Utilities.HasFlag(_rewardType, RewardType.Weapon))
    //    {
    //        if (Utilities.HasFlag(_rewardType, RewardType.Ability))
    //        {
    //            return GetLoot_WeaponOrAbilityDrop(_luckBonus);
    //        }
    //        else
    //        {
    //            return GetLoot_WeaponDrop(_luckBonus);
    //        }
    //    }
    //    else if (Utilities.HasFlag(_rewardType, RewardType.Ability))
    //    {
    //        if (Utilities.HasFlag(_rewardType, RewardType.Weapon))
    //        {
    //            return GetLoot_WeaponOrAbilityDrop(_luckBonus);
    //        }
    //        else
    //        {
    //            return GetLoot_AbilityDrop(_luckBonus);
    //        }
    //    }

    //    return null;
    //}
    //public GameObject GetLoot_KillDrop(float luckBonus)
    //{
    //    return killDropLootTable.GetLoot(luckBonus);
    //}
    //public GameObject GetLoot_WeaponDrop(float luckBonus)
    //{
    //    return weaponLootTable.GetLoot(luckBonus);
    //}
    //public GameObject GetLoot_AbilityDrop(float luckBonus)
    //{
    //    return abilityLootTable.GetLoot(luckBonus);
    //}
    //public GameObject GetLoot_WeaponOrAbilityDrop(float luckBonus)
    //{

    //    GameObject returnObj = null;

    //    if (UnityEngine.Random.value < weaponChance)
    //    {
    //        returnObj = GetLoot_WeaponDrop(luckBonus);

    //        if (returnObj == null)
    //            returnObj = GetLoot_AbilityDrop(luckBonus);
    //    }
    //    else
    //    {
    //        returnObj = GetLoot_AbilityDrop(luckBonus);

    //        if (returnObj == null)
    //            returnObj = GetLoot_WeaponDrop(luckBonus);
    //    }


    //    return returnObj;
    //}

    public GameObject GetItem(ListDefinitionName listName)
    {
        return GetItem(listName, 0f);
    }
    public GameObject GetItem(ListDefinitionName listName, float luckBonus)
    {
        ItemPoolListDefinition listDefinition = GetItemPoolListDefinition(listName);

        if (listDefinition == null)
        {
            return null;
        }


        return listDefinition.GetItem(luckBonus);

    }
    ItemPoolListDefinition GetItemPoolListDefinition(ListDefinitionName listName)
    {
        for (int i = 0; i < m_ItemPoolDefinitions.Count; i++)
        {
            if (m_ItemPoolDefinitions[i].ListName == listName)
            {
                return m_ItemPoolDefinitions[i];
            }
        }

        return null;
    }

    public void DropLoot(GameObject droppingObject)
    {

        if (droppingObject == null)
            return;

        Health _health = droppingObject.GetComponent<Health>();

        float luckBonus = (_health == null || _health.LastAttacker != Player.transform) ? 0f : pController.GetStatValue(StatType.Luck);



        GameObject newItem = null; // GetLoot_KillDrop(luckBonus);

        if (newItem == null)
            return;

        Vector3 launchVector = UnityEngine.Random.insideUnitSphere * LAUNCH_POWER;

        if (launchVector.y < 0)
            launchVector.y *= -1;


        newItem.transform.position = droppingObject.transform.position;
        newItem.SetActive(true);
    }

    public double GetPoolWeight(PoolWeightIdentifier _weightIdentifier)
    {
        switch (_weightIdentifier)
        {
            case PoolWeightIdentifier.RankWhiteWeight:
                return Values.WEIGHT_RANK_WHITE;

            //case PoolWeightIdentifier.RankGreen:
            //    return Values.WEIGHT_RANK_GREEN;

            case PoolWeightIdentifier.RankBlueWeight:
                return Values.WEIGHT_RANK_BLUE;

            case PoolWeightIdentifier.RankYellowWeight:
                return Values.WEIGHT_RANK_YELLOW;

            case PoolWeightIdentifier.RankPurpleWeight:
                return Values.WEIGHT_RANK_PURPLE;

            //case PoolWeightIdentifier.RankOrange:
            //    return Values.WEIGHT_RANK_ORANGE;

            //case PoolWeightIdentifier.RankCyan:
            //    return Values.WEIGHT_RANK_CYAN;

            case PoolWeightIdentifier.WeaponWeight:
                return Values.WEIGHT_WEAPONS;

            case PoolWeightIdentifier.AbilityWeight:
                return Values.WEIGHT_ABILITIES;

            case PoolWeightIdentifier.ToolWeight:
                return Values.WEIGHT_TOOLS;

            default:
                throw new NotImplementedException();
        }
    }

    /*
    void ChangeScore(int delta){
		score += delta;

        if(UIManager.Instance != null)
            UIManager.Instance.UpdateScore(Score);
	}
    */

    public void ChangeGodFavor(float delta)
    {
        if (OnFavorChange != null)
            OnFavorChange();


        DeityFavor += delta;
    }




    //void LoadLootTables()
    //{
    //    lootDictionary.Clear();


    //    //Map names to object prefabs
    //    UnityEngine.Object[] _objects = Resources.LoadAll(IN_GAME_OBJECT_FOLDER_PATH, typeof(GameObject));

    //    for (int i = 0; i < _objects.Length; i++)
    //    {
    //        GameObject obj = _objects[i] as GameObject;

    //        if (obj == null || lootDictionary.ContainsKey(obj.name))
    //            continue;

    //        lootDictionary.Add(obj.name, obj);
    //    }


    //    encounteredItems.Clear();


    //    //Load xml file
    //    TextAsset databaseTextAsset = Resources.Load(LOOT_DATABASE_PATH) as TextAsset;

    //    if (databaseTextAsset != null)
    //    {
    //       lootItems = ItemContainer.LoadFromText(databaseTextAsset.text);   //LOOT_DATABASE_PATH);

    //        if (showDebug)
    //        {
    //            Debug.Log("Item database loaded");


    //            Item[] _items = lootItems.items;
    //            for (int i = 0; i < _items.Length; i++)
    //            {
    //                Debug.Log($"Loaded: {_items[i]}");
    //            }
    //        }
    //    }
    //}






    //#region Element Table

    //Dictionary<ElementTableKey, float> elementTable = new Dictionary<ElementTableKey, float>();

    //struct ElementTableKey
    //{
    //    public readonly ElementType Element1;
    //    public readonly ElementType Element2;

    //    public ElementTableKey(ElementType e1, ElementType e2)
    //    {
    //        Element1 = e1;
    //        Element2 = e2;
    //    }
    //}

    //void LoadElementTable()
    //{
    //    elementTable.Clear();


    //    ElementType[] _elements = (ElementType[])Enum.GetValues(typeof(ElementType));

    //    for (int i = 0; i < _elements.Length; i++)
    //    {
    //        for (int k = 0; k < _elements.Length; k++)
    //        {
    //            ElementTableKey _key = new ElementTableKey(_elements[i], _elements[k]);
    //            elementTable.Add(_key, 0f);
    //        }
    //    }

    //}

    //public float GetElementInteraction(ElementType e1, ElementType e2)
    //{
    //    return elementTable[new ElementTableKey(e1,e2)];
    //}

    //#endregion



    #region Accessors

    public GameObject Player
    {
        get { return player; }
    }
    public PlayerController PlayerController
    {
        get { return pController; }
    }

    public GameState CurrentGameState
    {
        get { return currentGameState; }
    }

    public int CurrentLevel
    {
        get { return currentLevel; }
    }
    public int LevelPoints
    {
        get { return currentLevelPoints; }
    }
    public float DeityFavor
    {
        get { return currentDeityFavor; }
        private set { currentDeityFavor = Mathf.Clamp(value, -DEITY_FAVOR_MAX, DEITY_FAVOR_MAX); }
    }


    public bool CanStartDisaster
    {
        get { return canStartDisaster; }
    }
    /*
    public float DisasterChance
    {
        get { return disasterChanceBase + (-godFavor * disasterChanceFavorBonus); }
    }*/
    #endregion



    void ValidateItemPoolDefinitions()
    {
        HashSet<ListDefinitionName> encounteredNameSet = new HashSet<ListDefinitionName>();
        HashSet<ListDefinitionName> referenceSet = new HashSet<ListDefinitionName>((ListDefinitionName[])Enum.GetValues(typeof(ListDefinitionName)));

        for (int i = 0; i < m_ItemPoolDefinitions.Count; i++)
        {
            if (encounteredNameSet.Contains(m_ItemPoolDefinitions[i].ListName))
            {
                m_ItemPoolDefinitions.RemoveAt(i);
                i--;
                continue;
            }
            else
            {
                encounteredNameSet.Add(m_ItemPoolDefinitions[i].ListName);
                referenceSet.Remove(m_ItemPoolDefinitions[i].ListName);

                m_ItemPoolDefinitions[i].Validate();
            }
        }

        HashSet<ListDefinitionName>.Enumerator enumerator = referenceSet.GetEnumerator();

        while (enumerator.MoveNext())
        {
            m_ItemPoolDefinitions.Add(new ItemPoolListDefinition(enumerator.Current));
        }


        m_ItemClassProbabilities.Validate(true);

    }
    void OnValidate()
    {
        ValidateItemPoolDefinitions();
    }
}
