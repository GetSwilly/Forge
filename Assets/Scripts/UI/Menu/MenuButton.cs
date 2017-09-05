using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{

    public delegate void ClickAction(MenuButton clickedButton);
    public ClickAction OnButtonClicked;

    [SerializeField]
    protected Text m_MainText;

    [SerializeField]
    protected Text m_SecondaryText;

    [SerializeField]
    protected Image m_Image;
    
    protected string title;
  


    protected Menu m_Menu;
    protected Button m_Button;


    protected virtual void Awake()
    {
        m_Button = GetComponent<Button>();
    }

    public virtual void Initialize(Menu _menu)
    {
        Initialize(_menu, null, "");
    }
    public virtual void Initialize(Menu _menu, Sprite _sprite, string _title)
    {
        m_Menu = _menu;

        SetImage(_sprite);
        SetTitle(_title);
    }
    public virtual void Activate()
    {
        m_Button.interactable = true;
    }
    public virtual void Deactivate()
    {
        m_Button.interactable = false;
    }

    public virtual void SetTitle(string _title)
    {
        if (m_MainText == null || _title == null)
            return;

        m_MainText.text = _title;
    }
    public virtual void SetImage(Sprite _sprite)
    {
        if (m_Image == null || _sprite == null)
            return;

        m_Image.sprite = _sprite;
    }

    public virtual void ButtonClicked()
    {
        if (OnButtonClicked != null)
            OnButtonClicked.Invoke(this);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        m_Menu.SelectedButton = this;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        m_Menu.SelectedButton = null;
    }


    public string MainText
    {
        get
        {
            if (m_MainText == null)
                return "";

            return m_MainText.text;
        }
        set
        {
            if (m_MainText == null)
                return;

            m_MainText.text = value;
        }
    }
    public string SecondaryText
    {
        get
        {
            if (m_SecondaryText == null)
                return "";

            return m_SecondaryText.text;
        }
        set
        {
            if (m_SecondaryText == null)
                return;

            m_SecondaryText.text = value;
        }
    }
}
