using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoFade : MonoBehaviour {

	public enum FadeMethod { Color, Transparent, CanvasGroup }
    public enum FadeCycle { Single, SingleLoop, LimitedLoop, ContinuousLoop }


    [SerializeField]
    public FadeMethod m_Method;

    [SerializeField]
    public FadeCycle m_Cycle;

    [SerializeField]
    int limitedLoopCount = 3;


    [SerializeField]
    float fadeTime;

    [SerializeField]
    float fadeDelay;

    [SerializeField]
    bool fadeOnAwake = false;

    [SerializeField]
    bool disableOnCompletion = true;


    [SerializeField]
    [Range(0f, 1f)]
    float m_Alpha = 0f;



    [SerializeField]
    Color m_StartColor;

    [SerializeField]
    Color m_EndColor;

    //[SerializeField]
    //Material m_Material;

    [SerializeField]
    MaskableGraphic m_Graphic;

 

    




    [SerializeField]
    CanvasGroup m_CanvasGroup;



    //Original Variables
    [SerializeField]
    [Range(0f,1f)]
    float resetAlpha = 1;

    [SerializeField]
    Color resetColor;




    bool isComplete = false;

    void Awake()
    {
        if (m_CanvasGroup == null)
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
        }
    }
    void OnEnable()
    {
        if (fadeOnAwake)
        {
            Fade();
        }
    }






    //public void StartFade()
    //{
    //    if (m_Material != null)
    //    {
    //        StartCoroutine(FadeMaterial());
    //    }
    //    else if (m_Graphic != null)
    //    {
    //        StartCoroutine(FadeGraphic());
    //    }
    //}



    //IEnumerator FadeMaterial()
    //{
    //    Color currentColor = startColor;
    //    m_Material.color = currentColor;

    //    for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / FadeInTime)
    //    {
    //        currentColor = Color.Lerp(currentColor, endColor, t);
    //        m_Material.color = currentColor;
    //        yield return null;
    //    }

    //    /*
    //    while (Vector4.Distance(currentColor, endColor) > MIN_COLOR_DISTANCE)
    //    {
    //        yield return null;
    //        currentColor = Color.Lerp(currentColor, endColor, (1 / fadeInTime);//) * Time.deltaTime);
    //        m_Material.color = currentColor;
    //    }
    //    */
    //    currentColor = endColor;
    //    m_Material.color = currentColor;

    //    yield return new WaitForSeconds(fadeDelay);

    //    /*
    //    while (Vector4.Distance(currentColor, startColor) > MIN_COLOR_DISTANCE)
    //    {
    //        yield return null;
    //        currentColor = Color.Lerp(currentColor, startColor, 1 / fadeOutTime);//) * Time.deltaTime);
    //        m_Material.color = currentColor;
    //    }
    //    */

    //    for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / FadeOutTime)
    //    {
    //        currentColor = Color.Lerp(currentColor, startColor, t);
    //        m_Material.color = currentColor;
    //        yield return null;
    //    }


    //    currentColor = startColor;
    //    m_Material.color = currentColor;

    //    if (disableOnCompletion)
    //        gameObject.SetActive(false);
    //}



















    public void Fade(Color ogColor, Color a, Color b)
    {
        Method = FadeMethod.Color;

        resetColor = ogColor;

        StartColor = a;
        EndColor = b;

        Fade();
    }
    public void Fade(float ogAlpha) { Fade(ogAlpha, Alpha); }
    public void Fade(float ogAlpha, FadeMethod _method) { Fade(ogAlpha, Alpha, _method, Cycle); }
    public void Fade(float ogAlpha, float desiredAlpha) { Fade(ogAlpha, desiredAlpha, Method, Cycle); }
    public void Fade(float ogAlpha, float desiredAlpha, FadeMethod _method, FadeCycle _cycle)
    {
        Method = _method;
        Cycle = _cycle;

        ogAlpha = Mathf.Clamp01(ogAlpha);

        resetAlpha = ogAlpha;
        Alpha = desiredAlpha;

        Fade();
    }
    void Fade()
    {
        StopAllCoroutines();
        


        IEnumerator routine = null;

        switch (Method)
        {
            case FadeMethod.CanvasGroup:
                routine = FadeToAlphaRoutine(Alpha);
                break;
            case FadeMethod.Color:
                break;
            case FadeMethod.Transparent:
                break;
        }



        if (routine != null)
        {
            switch (Cycle)
            {
                case FadeCycle.Single:
                    StartCoroutine(Single(routine));
                    break;
                case FadeCycle.SingleLoop:
                    StartCoroutine(LimitedLoop(routine,1));
                    break;
                case FadeCycle.LimitedLoop:
                    StartCoroutine(LimitedLoop(routine, LimitedLoopCount));
                    break;
                case FadeCycle.ContinuousLoop:
                    StartCoroutine(ContinuousLoop(routine));
                    break;
            }
           
        }
    }
    IEnumerator FadeBackRoutine()
    {
        switch (Method)
        {
            case FadeMethod.CanvasGroup:
                yield return StartCoroutine(FadeToAlphaRoutine(resetAlpha));
                break;
            case FadeMethod.Color:
                break;
            case FadeMethod.Transparent:
                break;
        }
    }
    public void Reset()
    {
        throw new NotImplementedException();
    }


    IEnumerator Single(IEnumerator routine)
    {
        yield return StartCoroutine(routine);

        if (DisableOnCompletion)
        {
            gameObject.SetActive(false);
        }
    } 
    IEnumerator LimitedLoop(IEnumerator routine, int count)
    {
        if (count < 0)
            count = 0;


        for(int i = 0; i < count; i++)
        {
            yield return StartCoroutine(routine);
            yield return new WaitForSeconds(FadeDelay);
            yield return StartCoroutine(FadeBackRoutine());
        }


        if (DisableOnCompletion)
        {
            gameObject.SetActive(false);
        }
    }
    IEnumerator ContinuousLoop(IEnumerator routine)
    {
        while (true)
        {
            yield return StartCoroutine(routine);
            yield return new WaitForSeconds(FadeDelay);
            yield return StartCoroutine(FadeBackRoutine());
        }
    }





    //public void FadeIn()
    //{
    //    StopAllCoroutines();
    //    StartCoroutine(FadeRoutine(1f, false));
    //}
    //public void FadeOut(bool shouldDisableOnCompletion)
    //{
    //    StopAllCoroutines();
    //    StartCoroutine(FadeRoutine(0f, shouldDisableOnCompletion));
    //}



    IEnumerator FadeToAlphaRoutine(float desiredAlpha)
    {
        Console.Out.WriteLine("Fading to " + desiredAlpha);


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
    }

    IEnumerator FadeGraphic()
    {
        Color currentColor = StartColor;
        m_Graphic.color = currentColor;

        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / FadeTime)
        {
            currentColor = Color.Lerp(StartColor, EndColor, t);
            m_Graphic.color = currentColor;
            yield return null;
        }

        currentColor = EndColor;
        m_Graphic.color = currentColor;

        yield return new WaitForSeconds(FadeDelay);

        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / FadeTime)
        {
            currentColor = Color.Lerp(EndColor, StartColor, t);
            m_Graphic.color = currentColor;
            yield return null;
        }
        currentColor = StartColor;
        m_Graphic.color = currentColor;


        if (disableOnCompletion)
            gameObject.SetActive(false);

    }





    public FadeMethod Method
    {
        get { return m_Method; }
        set { m_Method = value; }
    }
    public FadeCycle Cycle
    {
        get { return m_Cycle; }
        set { m_Cycle = value; }
    }

   


    public bool IsComplete
    {
        get { return isComplete; }
    }



    public float FadeTime
    {
        get { return fadeTime; }
        set
        {
            fadeTime = value;
            fadeTime = Mathf.Max(0f, fadeTime);
        }
    }
    public float FadeDelay
    {
        get { return fadeDelay; }
        set
        {
            fadeDelay = value;
            fadeDelay = Mathf.Max(0f, fadeDelay);
        }
    }
    public float Alpha
    {
        get { return m_Alpha; }
        set { m_Alpha = Mathf.Clamp01(m_Alpha); }
    }
    public int LimitedLoopCount
    {
        get { return limitedLoopCount; }
        set
        {
            limitedLoopCount = value;

            if (limitedLoopCount < 0)
                limitedLoopCount = 0;
        }
    }
    public bool FadeOnAwake
    {
        get { return fadeOnAwake; }
        set { fadeOnAwake = value; }
    }
    public bool DisableOnCompletion
    {
        get { return disableOnCompletion; }
        set { disableOnCompletion = value; }
    }











    public Color StartColor
    {
        get { return m_StartColor; }
        set { m_StartColor = value; }
    }
    public Color EndColor
    {
        get { return m_EndColor; }
        set { m_EndColor = value; }
    }



    void OnValidate()
    {
        FadeTime = FadeTime;
        FadeDelay = FadeDelay;
    }

}
