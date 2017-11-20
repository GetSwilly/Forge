using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ProgressBarController : DisplayUI
{

    static readonly float FADE_TIME = 0.7f;

    [SerializeField]
    Color m_Color = Color.white;

    [SerializeField]
    Text m_Text;

    public bool showPercentage = false;

    [SerializeField]
    float updateSpeed = 0.4f;

    float desiredPercentage = 1f;
    float curPercentage = 1f;


    [SerializeField]
    Image progessBarImage;

    bool isFading = false;


    Camera mainCam;




    void Start()
    {
        mainCam = Camera.main;

        if (progessBarImage != null)
        {
            curPercentage = progessBarImage.fillAmount;
        }
    }
    void OnEnable()
    {
        curPercentage = 1f;
        progessBarImage.fillAmount = 1f;
    }

    protected override void Update()
    {
        if (curPercentage != desiredPercentage)
        {
            float diff = desiredPercentage - curPercentage;

            if (diff > 0)
            {
                curPercentage += UpdateSpeed * Time.deltaTime;

                if (curPercentage > desiredPercentage)
                    curPercentage = desiredPercentage;
            }
            else
            {
                curPercentage -= UpdateSpeed * Time.deltaTime;

                if (curPercentage < desiredPercentage)
                    curPercentage = desiredPercentage;
            }

            SetFillPercentage(curPercentage);
        }

    }

    public void SetPercentage(float percentage)
    {
        SetPercentage(percentage, false);
    }
    public override void SetPercentage(float pctg, bool setImmediately)
    {
        if (setImmediately)
        {
            SetFillPercentage(pctg);
        }
        else
        {
            SetDesiredPercentage(pctg);
        }
    }

    void SetDesiredPercentage(float _desired)
    {
        desiredPercentage = Mathf.Clamp01(_desired);
    }
    void SetFillPercentage(float _fill)
    {
        curPercentage = Mathf.Clamp01(_fill);

        if (progessBarImage == null)
            return;


        progessBarImage.fillAmount = curPercentage;

        int percent = (int)(progessBarImage.fillAmount * 100);

        if (showPercentage)
            SetText(percent.ToString() + "%");
    }

    public override void SetText(string txt)
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
    protected float UpdateSpeed
    {
        get { return updateSpeed; }
        private set { updateSpeed = Mathf.Clamp(value, 0f, value); }
    }


    void OnValidate()
    {
        Color = Color;
        UpdateSpeed = UpdateSpeed;
    }
}
