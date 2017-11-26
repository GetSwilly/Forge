using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ProgressBarController : MonoBehaviour
{
    [SerializeField]
    MovementType m_MovementType = MovementType.MoveTowards;

    [SerializeField]
    Color m_Color = Color.white;

    [SerializeField]
    Text m_Text;

    public bool showPercentage = false;

    [SerializeField]
    float speed = 0.4f;

    [SerializeField]
    float smoothTime;

    float desiredPercentage = 1f;
    float currentPercentage = 1f;


    [SerializeField]
    Image progessBarImage;

    float currentVelocity;

    void Start()
    {
        if (progessBarImage != null)
        {
            currentPercentage = progessBarImage.fillAmount;
        }
    }
    void OnEnable()
    {
        currentPercentage = 1f;
        progessBarImage.fillAmount = 1f;
    }

    protected void Update()
    {
        switch (m_MovementType)
        {
            case MovementType.MoveTowards:
                currentPercentage = Mathf.MoveTowards(currentPercentage, desiredPercentage, Speed * Time.deltaTime);
                break;
            case MovementType.Lerp:
                currentPercentage = Mathf.Lerp(currentPercentage, desiredPercentage, Speed * Time.deltaTime);
                break;
            case MovementType.SmoothDamp:
                currentPercentage = Mathf.SmoothDamp(currentPercentage, desiredPercentage, ref currentVelocity, SmoothTime);
                break;
        }

        SetFillPercentage(currentPercentage);

    }



    public void SetPercentage(float percentage)
    {
        SetPercentage(percentage, false);
    }
    public void SetPercentage(float pctg, bool setImmediately)
    {
        if (setImmediately)
        {
            SetFillPercentage(pctg);
        }

        SetDesiredPercentage(pctg);
    }

    void SetDesiredPercentage(float _desired)
    {
        desiredPercentage = Mathf.Clamp01(_desired);
    }
    void SetFillPercentage(float _fill)
    {
        currentPercentage = Mathf.Clamp01(_fill);

        if (progessBarImage == null)
            return;


        progessBarImage.fillAmount = currentPercentage;

        int percent = (int)(progessBarImage.fillAmount * 100);

        if (showPercentage)
            SetText(percent.ToString() + "%");
    }

    public void SetText(string txt)
    {
        if (m_Text != null)
            m_Text.text = txt;
    }

    public Color Color
    {
        get { return m_Color; }
        set
        {
            m_Color = value;

            if (progessBarImage == null)
                return;

            progessBarImage.color = value;
        }
    }
    protected float Speed
    {
        get { return speed; }
        private set { speed = Mathf.Clamp(value, 0f, value); }
    }
    protected float SmoothTime
    {
        get { return smoothTime; }
        private set { smoothTime = Mathf.Clamp(value, 0f, value); }
    }

    void OnValidate()
    {
        Color = Color;

        Speed = Speed;
        SmoothTime = SmoothTime;
    }
}
