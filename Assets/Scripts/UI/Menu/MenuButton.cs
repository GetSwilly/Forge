using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButton : MonoBehaviour
{

    public delegate void ClickAction(MenuButton clickedButton);


    [SerializeField]
    protected Text mainText;

    [SerializeField]
    protected Text secondaryText;


    [SerializeField]
    protected Button mainButton;

    [SerializeField]
    protected Button secondaryButton;
    
    public ClickAction OnActionMain;
    public ClickAction OnActionSecondary;

    protected void Start()
    {
        if (mainButton != null)
        {
            mainButton.onClick.AddListener(() =>
            {
                if (OnActionMain != null)
                {
                    OnActionMain(this);
                }
            });
        }

        if (secondaryButton != null)
        {
            secondaryButton.onClick.AddListener(() =>
            {
                if (OnActionSecondary != null)
                {
                    OnActionSecondary(this);
                }
            });
        }
    }
    public void Initialize(string text1)
    {
        Initialize(text1, "");
    }
    public virtual void Initialize(string text1, string text2)
    {
        SetMainText(text1);
        SetSecondaryText(text2);
    }
    public virtual void Activate()
    {
        mainButton.interactable = true;
        secondaryButton.interactable = true;
    }
    public virtual void Deactivate()
    {
        mainButton.interactable = false;
        secondaryButton.interactable = false;
    }

    public void SetMainText(string text)
    {
        if (mainText == null)
            return;

        mainText.text = text;
    }
    public void SetSecondaryText(string text)
    {
        if (secondaryText == null)
            return;

        secondaryText.text = text;
    }
}
