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
    
    static readonly int START_LIVES_PLAYER = 1;
    
    static readonly float ENEMY_DEATH_WEIGHT = 1f;
    
    static readonly float ITEM_DROP_LAUNCH_POWER = .8f;
    static readonly Vector3 ITEM_DROP_OFFSET = new Vector3(0f,0.2f,0f);
    static readonly float ITEM_DROP_DELAY = 0.05f;
    
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
    int m_Lives = 0;


    [SerializeField]
    int currentLevelPoints = 0;
    

    int numKilled = 0;


    [SerializeField]
    public GameObject generatedObjectHolder;


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

        m_Lives = START_LIVES_PLAYER;
       
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

        pInput.CanEngage = false;
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
        pInput.CanEngage = false;
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
        
        yield return StartCoroutine(LevelController.Instance.GenerateLevel());
        
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
        pInput.CanEngage = true;

        Rigidbody rigid = player.GetComponent<Rigidbody>();

        if (rigid != null)
        {
            rigid.velocity = Vector3.zero;
        }

        pController.enabled = true;
        pInput.CanEngage = true;

        
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
        //CameraShake.Instance.ShakeMinor();
    }


    public void PlayerKilled(Health playerHealth)
    {
        PlayerKilled(playerHealth.gameObject);
    }
    public void PlayerKilled(GameObject playerObj)
    {
        m_Lives--;
        // ChangeScore(SCORE_ONPLAYERKILLED);

        if (m_Lives <= 0)
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

                    yield return new WaitForSeconds(ITEM_DROP_DELAY);
                }
            }
        }
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
        
        GameObject newItem = GetItem(ListDefinitionName.GeneralItems);

        if (newItem == null)
            return;

        Vector3 launchVector = UnityEngine.Random.insideUnitSphere * ITEM_DROP_LAUNCH_POWER;

        if (launchVector.y < 0)
            launchVector.y *= -1;

        Rigidbody _rigidbody = newItem.GetComponent<Rigidbody>();

        newItem.transform.position = droppingObject.transform.position + ITEM_DROP_OFFSET;
        newItem.SetActive(true);

        if(_rigidbody != null)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(launchVector, ForceMode.Impulse);
        }
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
    public int Lives
    {
        get { return m_Lives; }
        private set { m_Lives = Mathf.Clamp(value,0,value); }
    }
    public int CurrentLevel
    {
        get { return currentLevel; }
    }
    public int LevelPoints
    {
        get { return currentLevelPoints; }
    }
    #endregion
        

    void OnValidate()
    {
        Lives = Lives;

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
