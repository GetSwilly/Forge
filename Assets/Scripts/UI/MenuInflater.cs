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
    Menu menuPrefab;
    protected Menu m_Menu;

    [SerializeField]
    protected GameObject buttonPrefab;
    
    [SerializeField]
    bool disablePlayerMovement = false;
    
    [SerializeField]
    bool moveCameraToPosition = false;

    [SerializeField]
    Vector3 cameraOffset = new Vector3(0, 5, -10);

    [SerializeField]
    [Range(0f, 1f)]
    float cameraMoveLerp = 0.2f;

    [SerializeField]
    Vector3 cameraEulerAngles = new Vector3(0, 0, 0);

    [SerializeField]
    [Range(0f,1f)]
    float cameraRotateLerp = 0.2f;

    [SerializeField]
    [Range(1, 179)]
    int cameraFOV = 60;

    [SerializeField]
    [Range(0f, 180f)]
    float fovDelta = 20f;

    [SerializeField]
    bool requireLOS = true;

    [SerializeField]
    [EnumFlags]
    UIManager.Component m_UIComponents;

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


    public override bool Use(PlayerController _player)
    {
        if (isInflated)
            return false;

        activatingPlayer = _player;

       
        InflateMenu();

        return true;
    }
    public override bool Give(PlayerController player)
    {
      
        throw new NotImplementedException();
    }

    public override void Drop()
    {

    }




    public void InflateMenu()
    {
        if (menuPrefab == null || buttonPrefab == null || isInflated)
            return;


        isInflated = true;

        GameObject genObj = GameObject.Find("Generated Objects");

        m_Menu = Instantiate(menuPrefab) as Menu;
        m_Menu.transform.position = m_Transform.position;// MenuPosition;

       
        m_Menu.gameObject.transform.SetParent(genObj == null ? null : genObj.transform);
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



        CameraRelocate();


        StartCoroutine(DeflateCheckRoutine());

        AddButtons();
        DeflateUI();
    }
    public void DeflateMenu()
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





    protected abstract void AddButtons();
    


    protected void CameraRelocate()
    {
        if (!moveCameraToPosition)
            return;

        CameraFollow _follow = Camera.main.GetComponent<CameraFollow>();

        if (_follow != null)
            _follow.enabled = false;

        StartCoroutine(CameraRelocateRoutine(Camera.main));
    }
    private IEnumerator CameraRelocateRoutine(Camera cam)
    {
        Transform camTransform = cam.transform;

        while (true)
        {
            yield return null;

            camTransform.position = Vector3.Lerp(camTransform.position, m_Transform.position + cameraOffset, cameraMoveLerp);
            camTransform.rotation = Quaternion.Lerp(camTransform.rotation, Quaternion.Euler(cameraEulerAngles), cameraRotateLerp);
            
            cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, cameraFOV, fovDelta * Time.deltaTime);
        }
    }



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
