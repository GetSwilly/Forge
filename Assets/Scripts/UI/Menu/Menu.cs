using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : UIBase {
   
    [SerializeField]
    Transform pivotTransform;

    [SerializeField]
    Transform buttonParent;

    [SerializeField]
    Text menuTitle;
    
    
    [SerializeField]
    [Range(0f, 1f)]
    protected float lerpAmount = 0.25f;

    [SerializeField]
    float fadeTime = 1f;

    protected List<MenuButton> buttons = new List<MenuButton>();
    protected MenuInflater m_Inflater;


    MenuButton selectedButton;


    public delegate void AlertEvent();
    public delegate void ClickEvent(MenuButton clickedButton);
    public ClickEvent OnAccept;
    public AlertEvent OnCancel;


    public void OnDisable()
    {
        StopAllCoroutines();
    }
    

 
    public virtual void Inflate(MenuInflater _inflator)
    {
        m_Inflater = _inflator;

        pivotTransform.rotation = Quaternion.identity;
    }

    //public override void Deflate()
    //{
    //    Deflate(null);
    //}
    //public virtual void Deflate(MenuInflater _deflator)
    //{
    //    StopAllCoroutines();
    //}





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







    public MenuInflater Inflater
    {
        get { return m_Inflater; }
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
