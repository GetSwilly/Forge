using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class UnitUI : GenericUI {

    [SerializeField]
    float expFadeDelay = 3f;




    protected override void OnDisable()
    {
        base.OnDisable();

        transform.position = Vector3.zero;
    }



    void Update()
    {
        if (m_Follow.TargetTransform == null || !m_Follow.TargetTransform.gameObject.activeInHierarchy)
        {
            Terminate();
        }
    }


    public void Initialize(Transform ownerTransform, bool shouldShowLines, bool shouldShowExp)
    {
       
        if (ownerTransform == null)
        {
            gameObject.SetActive(false);
            return;
        }

        StopAllCoroutines();


        m_Follow.TargetTransform = ownerTransform;
        m_Transform.position = m_Follow.TargetTransform.position;
        m_Line.enabled = shouldShowLines;


        Inflate();



        Health _health = ownerTransform.GetComponent<Health>();
        UpdateAttribute("Health", _health == null ? 1f : _health.HealthPercentage, false);

        
        StartCoroutine(ExpFadeRoutine(false));
    }
    public void Terminate()
    {
        StopAllCoroutines();

        Deflate();
    }

   

    public void UpdateAttribute(string attrName, float pctg)
    {
        UpdateAttribute(attrName, pctg, false);
    }
    public void SetAttribute(string attrName, float pctg)
    {
        UpdateAttribute(attrName, pctg, true);
    }


    void UpdateExpProgressBar(float _percentage)
    {
        StartCoroutine(ExpFadeRoutine(true));
        UpdateAttribute("Experience", _percentage, false);
    }
    void SetExpProgressBar(float _percentage)
    {
        StartCoroutine(ExpFadeRoutine(true));
        UpdateAttribute("Experience", _percentage, true);
    }



    IEnumerator ExpFadeRoutine(bool shouldFadeIn)
    {
        if (shouldFadeIn)
        {
            m_Animator.SetTrigger("Exp Fade In");
        }

        yield return new WaitForSeconds(ExpFadeDelay);

        m_Animator.SetTrigger("Exp Fade Out");
    }
    /*
    void StartHealthBarFade(bool isFadingIn)
    {
        ProgressBarController healthBar = GetAttributeController("Health") as ProgressBarController;

        if (healthBar == null)
            return;


        if (isFadingIn)
        {
            healthBar.FadeIn(defaultFadeInTime);
        }
        else
        {
            healthBar.FadeOut(defaultFadeOutTime);
        }
    }
    void StartHandheldBarFade(bool isFadingIn)
    {
        ProgressBarController handheldBar = GetAttributeController("Handheld") as ProgressBarController;

        if (handheldBar == null)
            return;


        if (isFadingIn)
        {
            handheldBar.FadeIn(defaultFadeInTime);
        }
        else
        {
            handheldBar.FadeOut(defaultFadeOutTime);
        }
    }
    void StartExpBarFade()
    {
        StopCoroutine(FadeExpProgressBar());
        StartCoroutine(FadeExpProgressBar());
    }
    IEnumerator FadeExpProgressBar()
    {
        ProgressBarController expBar = GetAttributeController("Experience") as ProgressBarController;

        if (expBar != null)
        {

            expBar.FadeIn(expFadeInTime);

            while (expBar.IsFading)
            {
                yield return null;
            }

            yield return new WaitForSeconds(expFadeOutDelay);


            expBar.FadeOut(expFadeOutTime);
        }
    }





    IEnumerator FadeInAll(bool shouldShowExp)
    {
        ProgressBarController healthBar = GetAttributeController("Health") as ProgressBarController;
        ProgressBarController handheldBar = GetAttributeController("Handheld") as ProgressBarController;
        ProgressBarController expBar = GetAttributeController("Experience") as ProgressBarController;

        if (expBar != null)
            expBar.FadeOut(IMMEDIATE_FADE_TIME);

        if (handheldBar != null)
            handheldBar.FadeOut(IMMEDIATE_FADE_TIME);

        if (healthBar != null)
            healthBar.FadeOut(IMMEDIATE_FADE_TIME);


        while ((expBar != null && expBar.IsFading) || (handheldBar != null && handheldBar.IsFading) || (healthBar != null && healthBar.IsFading))
        {
            yield return null;
        }


        if (shouldShowExp)
            StartExpBarFade();

        if (handheldBar != null)
            handheldBar.FadeIn(defaultFadeInTime);

        if (healthBar != null)
            healthBar.FadeIn(defaultFadeInTime);

    }
    IEnumerator FadeOutAll()
    {
        ProgressBarController healthBar = GetAttributeController("Health") as ProgressBarController;
        ProgressBarController handheldBar = GetAttributeController("Handheld") as ProgressBarController;
        ProgressBarController expBar = GetAttributeController("Experience") as ProgressBarController;

        if (expBar != null)
            expBar.FadeOut();

        if (handheldBar != null)
            handheldBar.FadeOut(defaultFadeOutTime);

        if (healthBar != null)
            healthBar.FadeOut(defaultFadeOutTime);

        while ((expBar != null && expBar.IsFading) || (handheldBar != null && handheldBar.IsFading) || (healthBar != null && healthBar.IsFading))
        {
            yield return null;
        }

        
        gameObject.SetActive(false);
    }
    */





    public void UpdateAttributeUI(AttributeEffect _attribute, float _percentage)
    {
        string attributeString = _attribute.ToString();


        if (HasAttribute(attributeString))
        {
            if (_percentage <= 0)
            {
                RemoveAttribute(attributeString);
                return;
            }

            UpdateAttribute(attributeString, _percentage, false);
        }
        else if (_percentage > 0f)
        {
            GameObject attributeUI = GetPrefab("Attribute");
            ProgressBarController controller = attributeUI.GetComponent<ProgressBarController>();

            controller.Color = ColorManager.GetColor(_attribute.Attribute);
            attributeUI.SetActive(true);
            AddAttribute(new GenericUI.DisplayProperties(attributeString, new Orientation(Vector3.zero, Vector3.zero, Vector3.one),controller), GetParentTransform("Attribute"));
        }
    }



   
    public float ExpFadeDelay
    {
        get { return expFadeDelay; }
        set
        {
            expFadeDelay = Mathf.Clamp(value,0f,value);
        }
    }
   

    protected override void OnValidate()
    {
        base.OnValidate();

        ExpFadeDelay = ExpFadeDelay;
    }
}
