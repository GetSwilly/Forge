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
    float enemyUIDelay;

    [SerializeField]
    UnitController subscribedUnit;

    public delegate void ExposeUIEvent(UIManager.Component uiComponents);
    public delegate void UIUpdateEvent(UIManager.Component component, float percentage, float shouldSetImmediately);





    [HideInInspector]
    public static UIManager Instance { get; private set; }
    public void Awake()
    {
        if (Instance != null)
            Destroy(this);

        Instance = this;
    }

    void Start()
    {
        if (subscribedUnit != null)
        {
            Subscribe(subscribedUnit);
        }
    }

    public void Subscribe(UnitController unit)
    {
        if (unit == null)
            return;

        Unsubscribe();

        unit.UIAttributeChangedEvent += UIUpdate;

        if (unit.gameObject.activeInHierarchy)
        {
            unit.UpdateUI();
        }
    }
    public void Unsubscribe()
    {
        if (subscribedUnit == null)
            return;

        subscribedUnit.UIAttributeChangedEvent += UIUpdate;
    }

    private void UIUpdate(UnitController unit, UIEventArgs args)
    {
        switch (args.component)
        {
            case UIManager.Component.Health:
                healthUI.SetPercentage(args.percentage, args.shouldSetImmediately);
                break;
            case UIManager.Component.Experience:
                experienceUI.SetPercentage(args.percentage, args.shouldSetImmediately);
                break;
            case UIManager.Component.Handheld:
                handheldUI.SetPercentage(args.percentage, args.shouldSetImmediately);
                break;
            case UIManager.Component.NativeAbility:
                nativeAbilityUI.SetPercentage(args.percentage, args.shouldSetImmediately);
                break;
            case UIManager.Component.AuxiliaryAbility:
                auxiliaryAbilityUI.SetPercentage(args.percentage, args.shouldSetImmediately);
                break;
            case UIManager.Component.Merchant:
                break;
            case UIManager.Component.Enemy:
                StopAllCoroutines();

                if (args.percentage <= 0f)
                {
                    enemyHUD.SetActive(false);
                }
                else
                {
                    StartCoroutine(EnemyHUDVisibilityDelay());

                    enemyTitle.text = args.text;
                    enemyHealthBar.SetPercentage(args.percentage, enemyHUD.activeInHierarchy ? args.shouldSetImmediately : true);
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

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

        yield return new WaitForSeconds(enemyUIDelay);

        enemyHUD.SetActive(false);
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



    public bool IsLoadingScreenActive
    {
        get { return LoadingPanel.activeInHierarchy; }
    }
}
