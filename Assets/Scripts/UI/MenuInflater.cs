using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(InputListener))]
public abstract class MenuInflater : InteractableObject
{ 
    [SerializeField]
    protected Menu m_Menu;

    [SerializeField]
    protected GameObject buttonPrefab;
    
    bool isInflated = false;
    protected PlayerController activatingPlayer = null;
    InputListener m_Listener;

    protected override void Awake()
    {
        base.Awake();

        m_Listener = GetComponent<InputListener>();
    }

    public override bool Interact(PlayerController _player)
    {
        if (!CanInflateMenu())
            return false;

        activatingPlayer = _player;

        InflateMenu();

        return true;
    }

    public override void Drop()
    {

    }


    public void InflateMenu()
    {
        if (m_Menu == null || buttonPrefab == null || IsInflated)
            return;

        IsInflated = true;


        m_Menu.gameObject.SetActive(true);
        m_Menu.Inflate(this);

        UserInput _input = activatingPlayer.GetComponent<UserInput>();

        if (_input != null)
        {
            _input.CanMove = false;
            _input.CanEngage = false;
        }

        AddButtons();
        DeflateUI();
    }
    public virtual void DeflateMenu()
    {
        if (!IsInflated)
            return;

        IsInflated = false;


        StopAllCoroutines();

        m_Menu.Deflate();

        UserInput _input = activatingPlayer.GetComponent<UserInput>();
        if (_input != null)
        {
            _input.CanMove = true;
            _input.CanEngage = true;
        }
    }

    protected virtual bool CanInflateMenu()
    {
        return !IsInflated;
    }



    protected abstract void AddButtons();



    #region Accessors

    public override bool IsUsable
    {
        get { return base.IsUsable && (m_Menu == null || !m_Menu.gameObject.activeInHierarchy); }
    }
    public override bool IsUsableOutsideFOV
    {
        get { return false; }
    }

    protected bool IsInflated
    {
        get { return isInflated; }
        set
        {
            isInflated = value;
            m_Listener.enabled = isInflated;
        }
    }
    public int OptionCount
    {
        get { return 0; }
    }

    #endregion
}
