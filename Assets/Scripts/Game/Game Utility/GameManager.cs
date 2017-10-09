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
    int playerCredits;


    [SerializeField]
    List<ItemPoolListDefinition> m_ItemPoolDefinitions = new List<ItemPoolListDefinition>();


    Dictionary<string, GameObject> lootDictionary = new Dictionary<string, GameObject>();

    List<string> encounteredItems = new List<string>();


    [SerializeField]
    BalancingProbabilities m_ItemClassProbabilities = new BalancingProbabilities();
    
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
            default:
                numKilled++;
                EnemyKilled(unitHealth);
                break;
        }

        unitObj.SetActive(false);
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


    public void EnemyKilled(Health enemyHealth)
    {
        if (enemyHealth == null)
            return;

        //UnityEngine.Debug.Log("Enemy Killed: " + enemyHealth.gameObject);
        //UnityEngine.Debug.Log("Last Attacker: " + enemyHealth.LastAttacker);

        StartCoroutine(EnemyKilled_DropRoutine(enemyHealth));

        enemyHealth.gameObject.SetActive(false);
    }
    IEnumerator EnemyKilled_DropRoutine(Health _health)
    {
        if (_health != null && _health.LastAttacker != null && _health.LastAttacker.tag == "Player")
        {
            DeathDrop dropScript = _health.GetComponent<DeathDrop>();

            if (dropScript != null)
            {
                for (int i = 0; i < dropScript.NumberOfDrops; i++)
                {
                    DropLoot(_health.gameObject);

                    yield return new WaitForSeconds(DROP_DELAY);
                }
            }
        }
    }


    public bool CanChargeCredits(int delta)
    {
        return (Credits - delta) >= 0;
    }
    public bool ChargeCredits(int delta)
    {
        if (!CanChargeCredits(delta))
        {
            return false;
        }

        Credits -= delta;
        
        return true;
    }
    public bool CanModifyLevelPoints(int delta)
    {
        return LevelPoints + delta >= 0;
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


        GameObject newItem = GetItem(ListDefinitionName.GeneralItems);

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

    public void ChangeGodFavor(float delta)
    {
        if (OnFavorChange != null)
            OnFavorChange();


        DeityFavor += delta;
    }
    

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
    public int Credits
    {
        get { return playerCredits; }
        private set { playerCredits = value; }
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
        

    void OnValidate()
    {
        ValidateItemPoolDefinitions();
    }
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
}
