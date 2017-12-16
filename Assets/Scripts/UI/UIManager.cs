using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour
{

    [Flags]
    public enum Component
    {
        Health,
        Experience,
        Handheld,
        NativeAbility,
        AuxiliaryAbility,
        Merchant,
        Enemy
    }



    [SerializeField]
    GameObject InGamePanel;


    [SerializeField]
    AutoFade m_TitleAutoFade;

    [SerializeField]
    Text m_TitleText;

    [SerializeField]
    Text m_TitleText_Minor;



    [SerializeField]
    PauseMenuController PauseMenu;

    [SerializeField]
    GameObject LoadingPanel;

    [SerializeField]
    LoadingInfo loadingScreenInfo;


    [Space(5)]
    [Header("Player UI")]
    [Space(5)]

    [SerializeField]
    ProgressBarController healthUI;

    [SerializeField]
    ProgressBarController experienceUI;

    [SerializeField]
    Text levelUI;

    [SerializeField]
    Text levelPointsUI;

    [SerializeField]
    Text creditUI;

    [SerializeField]
    ProgressBarController handheldUI;

    [SerializeField]
    ProgressBarController nativeAbilityUI;

    [SerializeField]
    ProgressBarController auxiliaryAbilityUI;

    [SerializeField]
    GameObject enemyHUD;

    [SerializeField]
    Text enemyTitle;

    [SerializeField]
    ProgressBarController enemyHealthBar;

    [SerializeField]
    AllyUIController allyUI;

    [SerializeField]
    float fadeDelay;


    [SerializeField]
    SoundClip healthGained;

    [SerializeField]
    SoundClip healthLost;

    [SerializeField]
    SoundClip experienceGained;

    [SerializeField]
    SoundClip experienceLost;

    [SerializeField]
    SoundClip creditsGained;

    [SerializeField]
    SoundClip creditsLost;

    [SerializeField]
    SoundClip normalDamageAchieved;

    [SerializeField]
    SoundClip criticalDamageAchieved;


    UnitController subscribedUnit;
    GameObject enemyObject;

    [HideInInspector]
    public static UIManager Instance { get; private set; }
    public void Awake()
    {
        if (Instance != null)
            Destroy(this);

        Instance = this;
    }


    public void Subscribe(UnitController unit)
    {
        if (unit == null)
            return;

        Unsubscribe();

        unit.OnHealthChange += HealthChanged;
        unit.OnCreditsChange += CreditsChanged;
        unit.OnDamageAchieved += CasualtyAchieved;
        unit.OnStatLevelChanged += StatLevelChanged;


        PlayerController p = unit as PlayerController;

        if (p != null)
        {
            p.OnExpChange += ExperienceChanged;
            p.OnLevelChange += LevelChanged;
            p.OnLevelPointsChange += LevelPointsChanged;
            p.OnHandheldChange += HandheldChanged;
            p.OnNativeAbilityChange += NativeAbilityChanged;
            p.OnAuxiliaryAbilityChange += AuxiliaryAbilityChanged;
        }
        else
        {
            ExperienceChanged(0f, 0f);
            HandheldChanged("",0f);
            NativeAbilityChanged("", 0f);
            AuxiliaryAbilityChanged("", 0f);
        }

        subscribedUnit = unit;
        subscribedUnit.UpdateUI();
    }


    public void Unsubscribe()
    {
        if (subscribedUnit == null)
            return;

        subscribedUnit.OnHealthChange -= HealthChanged;
        subscribedUnit.OnCreditsChange -= CreditsChanged;
        subscribedUnit.OnDamageAchieved -= CasualtyAchieved;
        subscribedUnit.OnStatLevelChanged -= StatLevelChanged;

        PlayerController subscribedPlayer = subscribedUnit as PlayerController;

        if (subscribedPlayer != null)
        {
            subscribedPlayer.OnExpChange -= ExperienceChanged;
            subscribedPlayer.OnLevelChange -= LevelChanged;
            subscribedPlayer.OnLevelPointsChange += LevelPointsChanged;
            subscribedPlayer.OnHandheldChange -= HandheldChanged;
            subscribedPlayer.OnNativeAbilityChange -= NativeAbilityChanged;
            subscribedPlayer.OnAuxiliaryAbilityChange -= AuxiliaryAbilityChanged;
        }

        HealthChanged(0f, 0f);
        CreditsChanged(0f, 0f);
        CasualtyAchieved(null);

        ExperienceChanged(0f, 0f);
        HandheldChanged("",0f);
        NativeAbilityChanged("", 0f);
        AuxiliaryAbilityChanged("", 0f);
    }

    #region UI Events

    private void AuxiliaryAbilityChanged(string abilityName, float percentage)
    {
        auxiliaryAbilityUI.SetText(abilityName);
        auxiliaryAbilityUI.SetPercentage(percentage);
    }

    private void NativeAbilityChanged(string abilityName, float percentage)
    {
        nativeAbilityUI.SetText(abilityName);
        nativeAbilityUI.SetPercentage(percentage);
    }

    private void HandheldChanged(string handheldName, float percentage)
    {
        handheldUI.SetText(handheldName);
        handheldUI.SetPercentage(percentage);
    }

    private void StatLevelChanged(StatType type, int level)
    {
        throw new NotImplementedException();
    }

    private void CasualtyAchieved(Health casualtyHealth)
    {
        StopAllCoroutines();

        if (casualtyHealth == null || casualtyHealth.HealthPercentage <= 0f)
        {
            enemyHUD.SetActive(false);
        }
        else
        {
            StartCoroutine(EnemyHUDVisibilityDelay());

            Identifier identifier = casualtyHealth.GetComponent<Identifier>();
            string text = identifier == null ? "" : identifier.Name;

            enemyTitle.text = text;
            enemyHealthBar.SetPercentage(casualtyHealth.HealthPercentage, enemyObject == null || enemyObject != casualtyHealth.gameObject);

            enemyObject = casualtyHealth.gameObject;
        }


        if (SoundManager.Instance != null && casualtyHealth != null)
        {
            if (casualtyHealth.WasLastAttackCritical)
            {
                SoundManager.Instance.PlaySound(criticalDamageAchieved);
            }
            else
            {
                SoundManager.Instance.PlaySound(normalDamageAchieved);
            }
        }
    }

    private void CreditsChanged(float newValue, float valueDelta)
    {
        creditUI.text = newValue.ToString("N0");

        if (valueDelta > 0)
        {
            SoundManager.Instance.PlaySound(creditsGained);
        }
        else
        {
            SoundManager.Instance.PlaySound(creditsLost);
        }
    }

    private void HealthChanged(float newValue, float valueDelta)
    {
        healthUI.SetPercentage(newValue);

        if (valueDelta > 0)
        {
            SoundManager.Instance.PlaySound(healthGained);
        }
        else
        {
            SoundManager.Instance.PlaySound(healthLost);
        }
    }
    private void ExperienceChanged(float newValue, float valueDelta)
    {
        experienceUI.SetPercentage(newValue);

        if (valueDelta > 0)
        {
            SoundManager.Instance.PlaySound(experienceGained);
        }
        else
        {
            SoundManager.Instance.PlaySound(experienceLost);
        }
    }
    private void LevelChanged(float newValue)
    {
        levelUI.text = ((int)newValue).ToString();
    }

    private void LevelPointsChanged(float newValue)
    {
        levelPointsUI.text = ((int)newValue).ToString();
    }

    #endregion


    public void SetColors()
    {
        GameObject _player = GameManager.Instance.Player;

        if (_player == null)
            return;

        Renderer playerRenderer = _player.GetComponent<Renderer>();
        if (playerRenderer != null && playerRenderer.sharedMaterial != null)
        {
            //healthBar.barColor = playerRenderer.sharedMaterial.color;
            //expBar.barColor = playerRenderer.sharedMaterial.color;
            //weaponBar.barColor = Utilities.GetComplementaryColor(playerRenderer.sharedMaterial.color, 10);
        }
    }


    #region InGame

    public void InflateInGame()
    {
        //SetColors();

        InGamePanel.SetActive(true);
    }
    public void DeflateInGame()
    {
        InGamePanel.SetActive(false);
    }

    public void SetPause(bool pauseVal)
    {
        PauseMenu.SetPause(pauseVal);
    }


    public void ShowTitleText(string mainText)
    {
        ShowTitleText(mainText, "");
    }
    public void ShowTitleText(string mainText, string subText)
    {
        m_TitleText.text = mainText;
        m_TitleText_Minor.text = subText;

        m_TitleAutoFade.Fade(0f, 1f, AutoFade.FadeMethod.CanvasGroup, AutoFade.FadeCycle.SingleLoop);
    }

    IEnumerator EnemyHUDVisibilityDelay()
    {
        enemyHUD.SetActive(true);

        yield return new WaitForSeconds(fadeDelay);

        enemyTitle.text = "";
        enemyHealthBar.SetPercentage(1f, true);
        enemyObject = null;

        enemyHUD.SetActive(false);
    }

    public void AddAllyUI(GameObject allyObject)
    {
        allyUI.AddAllyUI(allyObject);
    }
    public void RemoveAllyUI(GameObject allyObject)
    {
        allyUI.RemoveAllyUI(allyObject);
    }

    #endregion


    #region Loading Screen

    public void InflateLoadingScreen(string info, float pctg)
    {
        InflateLoadingScreen(info);
        InflateLoadingScreen(pctg);
    }
    public void InflateLoadingScreen(string info)
    {

        LoadingPanel.SetActive(true);

        loadingScreenInfo.UpdateInfo(info);
        loadingScreenInfo.UpdateProgress(0f);
    }
    public void InflateLoadingScreen(float pctg)
    {
        LoadingPanel.SetActive(true);

        loadingScreenInfo.UpdateProgress(pctg);
    }



    public void DeflateLoadingScreen()
    {
        LoadingPanel.SetActive(false);
    }

    #endregion


    public void DeflateAll()
    {
        DeflateInGame();
        DeflateLoadingScreen();
        //DisableCenterScreenText();
    }


    public void CreateDynamicInfoScript(Vector3 position, int value, Color color)
    {
        if (ObjectPoolerManager.Instance != null)
        {
            GameObject infoObject = ObjectPoolerManager.Instance.DynamicInfoPooler.GetPooledObject();
            DynamicInfoScript infoScript = infoObject.GetComponentInChildren<DynamicInfoScript>();

            infoObject.transform.position = position;
            infoObject.SetActive(true);
            infoScript.Initialize(value, color);
        }
    }



    public bool IsLoadingScreenActive
    {
        get { return LoadingPanel.activeInHierarchy; }
    }
}
