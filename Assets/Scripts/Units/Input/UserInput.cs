using UnityEngine;
using System.Collections;


public class UserInput : MonoBehaviour
{
    static readonly float _MaxRayDistance = 100f;
    public static readonly string _NativeAbilityInputString = "Ability1";
    public static readonly string _AuxiliaryAbilityInputString = "Ability2";
    public static readonly string _HandheldPrimaryInputString = "Primary";
    public static readonly string _HandheldSecondaryInputString = "Secondary";
    public static readonly string _HandheldTertiaryInputString = "Tertiary";
    public static readonly string _InteractionInputString = "Interact";
    public static readonly string _CancelInputString = "Cancel";
    public static readonly string _ThrowInputString = "Throw";

    public enum WorldSpace { TwoD, ThreeD }
    public enum MovementRelativityType { Global, Camera, User };


    [SerializeField]
    WorldSpace m_WorldSpace = WorldSpace.TwoD;

    [SerializeField]
    MovementRelativityType inputRelativity = MovementRelativityType.User;

    [SerializeField]
    bool canMove = true;

    [SerializeField]
    bool canEngage = true;

    bool isThrowing;

    [SerializeField]
    LayerMask pointerHitMask;


    PlayerController player;
    Transform m_Transform;
    Camera mainCam;

    void Awake()
    {

        m_Transform = GetComponent<Transform>();

        player = GetComponent<PlayerController>();


        mainCam = Camera.main;
    }
    void Start()
    {
        isThrowing = false;
    }


    void Update()
    {
        if (isThrowing)
            return;

        if (CanEngage)
        {
            CheckEngagementInput();
        }
    }

    void CheckEngagementInput()
    {
        if (Input.GetButtonDown(_InteractionInputString))
        {
            player.Interact();
        }
        else if (canEngage)
        {
            bool isBusy = false;


            //Utility item interaction
            if (player.HasUtilityItem && ((Utilities.HasFlag(player.UtilityItem.InputType, InputType.Hold) && Input.GetButton(_ThrowInputString)) || (Utilities.HasFlag(player.UtilityItem.InputType, InputType.Press) && Input.GetButtonDown(_ThrowInputString))))
            {
                player.ActivateUtilityItem();
                isBusy = true;
            }
            else if (player.IsUsingUtility && player.HasUtilityItem)
            {
                player.DeactivateUtilityItem();
                isBusy = true;
            }

            //Native ability interaction
            if (!isBusy && player.HasNativeAbility && player.NativeAbility.CanUseAbility() && ((Utilities.HasFlag(player.NativeAbility.InputType, InputType.Hold) && Input.GetButton(_NativeAbilityInputString)) || (Utilities.HasFlag(player.NativeAbility.InputType, InputType.Press) && Input.GetButtonDown(_NativeAbilityInputString))))
            {
                player.ActivateNativeAbility();
                isBusy = true;
            }
            else if (player.HasNativeAbility)
            {
                player.DeactivateNativeAbility();
            }

            //Auxiliary ability interaction
            if (!isBusy && player.HasAuxiliaryAbility && player.AuxiliaryAbility.CanUseAbility() && ((Utilities.HasFlag(player.AuxiliaryAbility.InputType, InputType.Hold) && Input.GetButton(_AuxiliaryAbilityInputString)) || (Utilities.HasFlag(player.AuxiliaryAbility.InputType, InputType.Press) && Input.GetButtonDown(_AuxiliaryAbilityInputString))))
            {
                player.ActivateAuxiliaryAbility();
                isBusy = true;
            }
            else if (player.HasAuxiliaryAbility)
            {
                player.DeactivateAuxiliaryAbility();
            }

            //Handheld Primary interaction
            if (!isBusy && player.HasHandheld && player.HandheldItem.CanActivatePrimary() && ((Utilities.HasFlag(player.HandheldItem.PrimaryInputType, InputType.Hold) && Input.GetButton(_HandheldPrimaryInputString)) || (Utilities.HasFlag(player.HandheldItem.PrimaryInputType, InputType.Press) && Input.GetButtonDown(_HandheldPrimaryInputString))))
            {
                player.ActivateHandheldPrimary();
                isBusy = true;
            }
            else if (!isBusy && player.HasHandheld)
            {
                player.DeactivateHandheldPrimary();
            }

            //Handheld Secondary interaction
            if (!isBusy & player.HasHandheld && player.HandheldItem.CanActivateSecondary() && ((Utilities.HasFlag(player.HandheldItem.SecondaryInputType, InputType.Hold) && Input.GetButton(_HandheldSecondaryInputString)) || (Utilities.HasFlag(player.HandheldItem.SecondaryInputType, InputType.Press) && Input.GetButtonDown(_HandheldSecondaryInputString))))
            {
                player.ActivateHandheldSecondary();
                isBusy = true;
            }
            else if (!isBusy && player.HasHandheld)
            {
                player.DeactivateHandheldSecondary();
            }

            //Handheld Tertiary interaction
            if (!isBusy & player.HasHandheld && player.HandheldItem.CanActivateTertiary() && ((Utilities.HasFlag(player.HandheldItem.TertiaryInputType, InputType.Hold) && Input.GetButton(_HandheldTertiaryInputString)) || (Utilities.HasFlag(player.HandheldItem.TertiaryInputType, InputType.Press) && Input.GetButtonDown(_HandheldTertiaryInputString))))
            {
                player.ActivateHandheldTertiary();
                isBusy = true;
            }
            else if (!isBusy && player.HasHandheld)
            {
                player.DeactivateHandheldUtility();
            }




            /*else if(Input.GetButtonDown("Throw") && numConsumables > 0){
				StartCoroutine(IncreaseThrowPower());
			}*/
        }


    }

    void FixedUpdate()
    {

        if (canMove)
        {
            CheckMovementInput();
        }
    }

    void CheckMovementInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = Vector3.zero;
        Vector3 aimPoint = m_Transform.position;
        RaycastHit hit;

        // Create a ray from the mouse cursor on screen in the direction of the camera.
        Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);


        if (Physics.Raycast(camRay, out hit, _MaxRayDistance, pointerHitMask))
        {
            aimPoint = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            
            float distance;
            if (plane.Raycast(camRay, out distance))
            {
                aimPoint = camRay.GetPoint(distance);
            }
        }

        moveDir = Vector3.zero;

        switch (inputRelativity)
        {
            case MovementRelativityType.Global:
                moveDir = (Vector3.right * h) + (Vector3.forward * v);
                break;
            case MovementRelativityType.Camera:
                moveDir = (Camera.main.transform.right * h) + (Camera.main.transform.up * v);
                break;
            case MovementRelativityType.User:
                moveDir = (player.transform.right * h) + (player.transform.forward * v);
                break;
        }
        aimPoint.y = m_Transform.position.y;

        player.HandleInput(moveDir, aimPoint);
    }




    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }
    public bool CanEngage
    {
        get { return canEngage; }
        set { canEngage = value; }
    }


    public void OnValidate()
    {
        switch (m_WorldSpace)
        {
            case WorldSpace.TwoD:
                Cursor.lockState = CursorLockMode.None;
                break;
            case WorldSpace.ThreeD:
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }
    }
}
