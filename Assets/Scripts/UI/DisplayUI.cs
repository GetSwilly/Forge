using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CanvasGroup))]
public class DisplayUI : MonoBehaviour {
  
    protected Transform m_Transform;
   protected CanvasGroup m_CanvasGroup;



    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_CanvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void SetText(string txt) { }
    public virtual void SetPercentage(float pctg, bool setImmediately) { }



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



    //protected IEnumerator FadeRoutine(float desiredAlpha, bool shouldDisableOnCompletion)
    //{
    //    desiredAlpha = Mathf.Clamp01(desiredAlpha);


    //    while (!m_CanvasGroup.alpha.Equals(desiredAlpha))
    //    {
    //        yield return null;

    //        float delta = (1f / fadeTime) * Time.deltaTime;

    //        float currentAlphaDifference = desiredAlpha - m_CanvasGroup.alpha;


    //        if (currentAlphaDifference < 0)
    //        {
    //            delta *= -1f;
    //        }


    //        if (Mathf.Abs(delta) > Mathf.Abs(currentAlphaDifference))
    //        {
    //            delta = currentAlphaDifference;
    //        }


    //        m_CanvasGroup.alpha += delta;
    //    }

    //    m_CanvasGroup.alpha = desiredAlpha;

    //    if (shouldDisableOnCompletion)
    //    {
    //        gameObject.SetActive(false);
    //    }
    //}



    public virtual void SetColor(Color _color) { }

}
