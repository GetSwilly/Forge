using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Menu : MonoBehaviour {
    
    //static readonly float FADE_TIME = 0f;



    [SerializeField]
    Transform pivotTransform;

    [SerializeField]
    Transform buttonParent;

    [SerializeField]
    Text menuTitle;


    [SerializeField]
    List<Image> imagesToFade = new List<Image>();
    

    [SerializeField]
    [Range(0f, 1f)]
    protected float lerpAmount = 0.25f;

    [SerializeField]
    float fadeTime = 1f;

    protected List<MenuButton> buttons = new List<MenuButton>();
    //Transform myTransform;
    protected MenuInflater myInflater;


    MenuButton selectedButton;
    CanvasGroup m_CanvasGroup;


    public delegate void AlertEvent();
    public delegate void ClickEvent(MenuButton clickedButton);
    public ClickEvent OnAccept;
    public AlertEvent OnCancel;


    public void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
    }
    public void OnDisable()
    {
        StopAllCoroutines();
    }




 
    public virtual void Inflate(MenuInflater _inflator)
    {
        myInflater = _inflator;

        pivotTransform.rotation = Quaternion.identity;
        
        FadeIn();
    }

    public virtual void Deflate()
    {
        Deflate(null);
    }
    public virtual void Deflate(MenuInflater _deflator)
    {
        StopAllCoroutines();
        FadeOut();
    }





    public virtual void Accept()
    {
        if(OnAccept != null)
        {
            OnAccept(SelectedButton);
        }
    }
    public virtual void Cancel()
    {
        if(OnCancel != null)
        {
            OnCancel();
        }
    }





    public void SetText(string txt)
    {
        if (menuTitle != null)
            menuTitle.text = txt;
    }
    public virtual void AddButton(GameObject buttonObject)
    {
        buttonObject.SetActive(true);
        buttonObject.transform.SetParent(buttonParent);

        // Vector3 pos = buttonObject.transform.localPosition;
        //  pos.z = 0;
        // buttonObject.transform.localPosition = pos
        buttonObject.transform.localPosition = Vector3.zero;
        buttonObject.transform.localRotation = Quaternion.identity;
        buttonObject.transform.localScale = Vector3.one;
    }




    public void FadeIn()
    {
        StopAllCoroutines();

        m_CanvasGroup.alpha = 1f;
        //StartCoroutine(FadeRoutine(1f, false));
    }
    public void FadeOut()
    {
        StopAllCoroutines();

        m_CanvasGroup.alpha = 0f;
        this.gameObject.SetActive(false);
        //StartCoroutine(FadeRoutine(0f, true));
    }
    


    protected IEnumerator FadeRoutine(float desiredAlpha, bool shouldDisableOnCompletion)
    {
        desiredAlpha = Mathf.Clamp01(desiredAlpha);


        while (!m_CanvasGroup.alpha.Equals(desiredAlpha))
        {
            yield return null;

            float delta = (1f / fadeTime) * Time.deltaTime;

            float currentAlphaDifference = desiredAlpha - m_CanvasGroup.alpha;


            if (currentAlphaDifference < 0)
            {
                delta *= -1f;
            }


            if (Mathf.Abs(delta) > Mathf.Abs(currentAlphaDifference))
            {
                delta = currentAlphaDifference;
            }


            m_CanvasGroup.alpha += delta;
        }

        m_CanvasGroup.alpha = desiredAlpha;

        if (shouldDisableOnCompletion)
        {
            //Destroy(this.gameObject);
            this.gameObject.SetActive(false);
        }
    }
   















    public MenuInflater Inflater
    {
        get { return myInflater; }
    }
    public MenuButton SelectedButton
    {
        get
        {
            if (transform.childCount <= 0)
                return null;

            return transform.GetChild(transform.childCount - 1).GetComponent<MenuButton>();
            //return selectedButton;
        }
        set { selectedButton = value; }
    }
}
