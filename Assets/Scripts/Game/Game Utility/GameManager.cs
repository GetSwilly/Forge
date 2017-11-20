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

    static readonly float _ItemDropLaunchPower = 1.3f;
    static readonly Vector3 _ItemDropOffset = new Vector3(0f, 0.2f, 0f);
    static readonly float _ItemDropDelay = 0.05f;

    static readonly Vector3 HUB_WORLD_PLAYER_SPAWN = new Vector3(0f, .5f, 0f);

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


    int numKilled = 0;


    [SerializeField]
    public GameObject generatedObjectHolder;

    [SerializeField]
    GameObject player;

    PlayerController playerController;
    UserInput pInput;
    

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


        GameObject g = new GameObject("Generated Objects");
        g.transform.position = transform.position;

        //LoadLootTables();

        Subscribe(player);

        UIManager.Instance.ShowTitleText("FORGE");
    }

    void Subscribe(GameObject playerObject)
    {
        if (playerObject == null)
            return;

        Subscribe(playerObject.GetComponent<PlayerController>());
    }
    void Subscribe(PlayerController _player)
    {
        if (_player == null)
            return;


        playerController = _player;
        player = playerController.gameObject;
        pInput = _player.GetComponent<UserInput>();

        Health pHealth = player.GetComponent<Health>();
        if (pHealth != null)
        {
            pHealth.OnDamaged += PlayerDamaged;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.Subscribe(playerController);
        }
    }
    public void Unsubscribe()
    {
        throw new NotImplementedException();
    }

    public void StartGame(PlayerController _player)
    {
        Subscribe(_player);

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


        currentGameState = GameState.PLAYING;


        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetColors();
            UIManager.Instance.InflateInGame();
        }
    }


    //public void UnitDamaged(Health unitHealth)
    //{
    //    GameObject unitObj = unitHealth.gameObject;

    //    switch (unitObj.tag)
    //    {
    //        case "Player":
    //            PlayerDamaged(unitObj);
    //            break;
    //        default:
    //            break;
    //    }

    //    Transform attackerTransform = unitHealth.LastAttacker;

    //    if (attackerTransform != null)
    //    {
    //        UnitController attackerController = attackerTransform.GetComponent<UnitController>();

    //        if (attackerController != null)
    //        {
    //            ///attackerController.DamageAchieved(unitHealth);
    //        }
    //    }
    //}
    //public void UnitKilled(Health unitHealth)
    //{
    //    GameObject unitObj = unitHealth.gameObject;

    //    switch (unitObj.tag)
    //    {
    //        case "Player":
    //            PlayerKilled(unitObj);
    //            break;
    //        default:
    //            numKilled++;
    //            EnemyKilled(unitHealth);
    //            break;
    //    }

    //    unitObj.SetActive(false);
    //}


    public void PlayerDamaged(Health playerHealth)
    {
        CameraShake.Instance.StartShake();
    }


    public void PlayerKilled(Health playerHealth)
    {
        m_Lives--;
    }


    public void EnemyKilled(Health enemyHealth)
    {
        if (enemyHealth == null)
            return;

        //UnityEngine.Debug.Log("Enemy Killed: " + enemyHealth.gameObject);
        //UnityEngine.Debug.Log("Last Attacker: " + enemyHealth.LastAttacker);

        //StartCoroutine(EnemyKilled_DropRoutine(enemyHealth));

        enemyHealth.gameObject.SetActive(false);
    }
    //IEnumerator EnemyKilled_DropRoutine(Health _health)
    //{
    //    if (_health != null && _health.LastAttacker != null && _health.LastAttacker.tag == "Player")
    //    {
    //        DeathDrop dropScript = _health.GetComponent<DeathDrop>();

    //        if (dropScript != null)
    //        {
    //            for (int i = 0; i < dropScript.NumberOfDrops; i++)
    //            {
    //                DropLoot(_health.gameObject);

    //                yield return new WaitForSeconds(ITEM_DROP_DELAY);
    //            }
    //        }
    //    }
    //}

    public GameObject GetItem(ItemPoolDefinition listName)
    {
        return GetItem(listName, 0f);
    }
    public GameObject GetItem(ItemPoolDefinition listName, float luckBonus)
    {
        return ItemPoolManager.Instance.GetItem(listName, luckBonus);

    }
   
    public void DropLoot(GameObject droppingObject)
    {
        if (droppingObject == null)
            return;

        GameObject newItem = GetItem(ItemPoolDefinition.GeneralItems);

        if (newItem == null)
            return;

        Vector3 launchVector = UnityEngine.Random.insideUnitSphere * _ItemDropLaunchPower;

        if (launchVector.y < 0)
            launchVector.y *= -1;

        Rigidbody _rigidbody = newItem.GetComponent<Rigidbody>();

        newItem.transform.position = droppingObject.transform.position + _ItemDropOffset;
        newItem.SetActive(true);

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(launchVector, ForceMode.Impulse);
        }
    }

    public void DropItems(Transform droppingTransform, ItemPoolDefinition desiredListDefinition, int numberOfDrops)
    {
        if (droppingTransform == null || numberOfDrops <= 0)
            return;

        StartCoroutine(DropItemsRoutine(droppingTransform, desiredListDefinition, numberOfDrops));
    }
    IEnumerator DropItemsRoutine(Transform droppingTransform, ItemPoolDefinition desiredListDefinition, int numberOfDrops)
    {
        for (int i = 0; i < numberOfDrops; i++)
        {
            GameObject newItem = GetItem(desiredListDefinition);

            if (newItem == null)
                continue;

            Vector3 launchVector = UnityEngine.Random.insideUnitSphere.normalized * _ItemDropLaunchPower;

            if (launchVector.y < 0)
                launchVector.y *= -1;

            newItem.transform.position = droppingTransform.position + _ItemDropOffset;
            newItem.SetActive(true);

            Rigidbody _rigidbody = newItem.GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.AddForce(launchVector, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(_ItemDropDelay);
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
        get { return playerController; }
    }

    public GameState CurrentGameState
    {
        get { return currentGameState; }
    }
    public int Lives
    {
        get { return m_Lives; }
        private set { m_Lives = Mathf.Clamp(value, 0, value); }
    }
    public int CurrentLevel
    {
        get { return currentLevel; }
    }

    #endregion


    void OnValidate()
    {
        Lives = Lives;
    }
   
}
