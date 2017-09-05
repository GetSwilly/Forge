using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour {
    
    [Flags]
    public enum Component
    {
        Health,
        Ammo,
        Merchant
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
   
    

    [SerializeField]
    GameObject merchantPanel;

    [SerializeField]
    Text currencyText;



    public delegate void UIEvent(UIManager.Component uiComponents);

	




	[HideInInspector]
	public static UIManager Instance { get; private set; }
	public void Awake()
    {
		if(Instance != null)
			Destroy(this);
		
		Instance = this;
	}

    void Start()
    {
     
        //Utilities.ActivateAll(healthBar.gameObject);
        //Utilities.ActivateAll(expBar.gameObject);
        //Utilities.ActivateAll(weaponBar.gameObject);

        /*
        if (expBar != null)
        {
            GameObject g = expBar.gameObject;
            Image img = expBar.GetComponent<Image>();

            while (img != null)
            {
                expImages.Add(img);
                expOriginalColors.Add(img.color);

                g = g.transform.parent.gameObject;
                img = g.GetComponent<Image>();
            }

            StartCoroutine(FadeExpProgressBar());
        }
        */
    }


	
	public void SetColors()
    {
		GameObject _player = GameManager.Instance.Player;

		if(_player == null)
			return;

		Renderer playerRenderer = _player.GetComponent<Renderer>();
		if(playerRenderer != null && playerRenderer.sharedMaterial != null){
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


    
    public void InflateMerchantUI()
    {
        if (merchantPanel == null)
            return;

        merchantPanel.SetActive(true);
        currencyText.text = string.Format("Level Points Available: {0}", GameManager.Instance.LevelPoints);
    }
    public void DeflateMerchantUI()
    {
        if (merchantPanel == null)
            return;

            merchantPanel.SetActive(false);
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
