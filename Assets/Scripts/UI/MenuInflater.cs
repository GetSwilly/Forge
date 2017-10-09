using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(InputListener))]
public abstract class MenuInflater : InteractableObject
{
    static readonly float MAX_SIGHT_ANGLE = 180f;
    static readonly float PLAYER_SPEED_MULTIPLIER = 0.7f;
    static readonly float PLAYER_ROTATION_MULTIPLIER = 0.1f;


    [SerializeField]
    protected Menu m_Menu;

    [SerializeField]
    protected GameObject buttonPrefab;
    
    [SerializeField]
    bool disablePlayerMovement = false;
  
    [SerializeField]
    bool requireLOS = true;

    [SerializeField]
    string menuText = "";


    bool isInflated = false;
    protected PlayerController activatingPlayer = null;



    //void Start()
    //{
    //    if (inflateMerchantUI)
    //    {
    //        OnUI_Inflate += UIManager.Instance.InflateMerchantUI;
    //        OnUI_Deflate += UIManager.Instance.DeflateMerchantUI;
    //    }
    //}


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
        if (m_Menu == null || buttonPrefab == null || isInflated)
            return;

        isInflated = true;
        
        m_Menu.gameObject.SetActive(true);
        m_Menu.Inflate(this);
        m_Menu.SetText(menuText);

        /*
        for(int i = 0; i <  triggerScripts.Count; i++)
        {
            triggerScripts[i].enabled = false;
        }
        */

        if (disablePlayerMovement)
        {
            UserInput _input = activatingPlayer.GetComponent<UserInput>();

            if (_input != null)
                _input.enabled = false;
        }
        MovementController _movement = activatingPlayer.GetComponent<MovementController>();

        if (_movement != null)
        {
            _movement.AddRotationMultiplier(PLAYER_ROTATION_MULTIPLIER);
            _movement.AddSpeedMultiplier(PLAYER_SPEED_MULTIPLIER);
        }


        StartCoroutine(DeflateCheckRoutine());

        AddButtons();
        DeflateUI();
    }
    public virtual void DeflateMenu()
    {
        if (!isInflated)
            return;



        isInflated = false;
        

        StopAllCoroutines();

        m_Menu.Deflate();

        /*
        for (int i = 0; i < triggerScripts.Count; i++)
        {
            triggerScripts[i].enabled = true;
        }
        */

        CameraFollow _follow = Camera.main.GetComponent<CameraFollow>();

        if (_follow != null)
        {
            _follow.TargetTransform = activatingPlayer.transform;
            _follow.enabled = true;
        }

        UserInput _input = activatingPlayer.GetComponent<UserInput>();

        if (_input != null)
            _input.enabled = true;

      

        MovementController _movement = activatingPlayer.GetComponent<MovementController>();

        if (_movement != null)
        {
            _movement.RemoveRotationMultiplier(PLAYER_ROTATION_MULTIPLIER);
            _movement.RemoveSpeedMultiplier(PLAYER_SPEED_MULTIPLIER);
        }

    }
    
    protected virtual bool CanInflateMenu()
    {
        return !isInflated;
    }



    protected abstract void AddButtons();
    

    IEnumerator DeflateCheckRoutine()
    {
        while (true)
        {
            yield return null;

            if (requireLOS && !activatingPlayer.CanSee(m_Transform, MAX_SIGHT_ANGLE))
                break;
        }

        DeflateMenu();
    }
   
    

    #region Accessors
    
    public override bool IsUsable
    {
        get { return base.IsUsable && (m_Menu == null || !m_Menu.gameObject.activeInHierarchy); }
    }
    public override bool IsUsableOutsideFOV
    {
        get { return false; }
    }

    public int OptionCount
    {
        get { return 0; }
    }
    
    #endregion
}
