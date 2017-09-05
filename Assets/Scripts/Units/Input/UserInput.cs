using UnityEngine;
using System.Collections;


public class UserInput : MonoBehaviour {

    static readonly float MAX_DISTANCE = 100f;


    public enum WorldSpace { TwoD, ThreeD }
    public enum MovementRelativityType { Global, Camera, User};


    [SerializeField]
    WorldSpace m_WorldSpace = WorldSpace.TwoD;

    [SerializeField]
    MovementRelativityType inputRelativity = MovementRelativityType.User;

    [SerializeField]
    bool canAttack;

    bool isThrowing;

    [SerializeField]
    LayerMask hitMask;


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



        int type = Input.GetButtonDown("Use") ? 1 : -1; // (Input.GetButtonDown("Give") ? 2 : -1);

        if (type != -1)
            player.Interact(type);

        if (canAttack)
        {
            bool isBusy = false;


            //Utility item interaction
            if (player.HasUtilityItem && ((Utilities.HasFlag(player.UtilityItem.InputType, InputType.Hold) && Input.GetButton("Throw")) || (Utilities.HasFlag(player.UtilityItem.InputType, InputType.Press) && Input.GetButtonDown("Throw"))))
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
            if (!isBusy && player.HasNativeAbility && player.NativeAbility.CanUseAbility() && ((Utilities.HasFlag(player.NativeAbility.InputType, InputType.Hold) && Input.GetButton("Native Ability")) || (Utilities.HasFlag(player.NativeAbility.InputType , InputType.Press) && Input.GetButtonDown("Native Ability"))))
            {
                player.ActivateNativeAbility();
                isBusy = true;
            }
            else if (player.HasNativeAbility)
            {
                player.DeactivateNativeAbility();
            }

            //Auxiliary ability interaction
            if (!isBusy && player.HasAuxiliaryAbility && player.AuxiliaryAbility.CanUseAbility() && ((Utilities.HasFlag(player.AuxiliaryAbility.InputType,InputType.Hold) && Input.GetButton("Auxiliary Ability")) || (Utilities.HasFlag(player.AuxiliaryAbility.InputType, InputType.Press) && Input.GetButtonDown("Auxiliary Ability"))))
            {
                player.ActivateAuxiliaryAbility();
                isBusy = true;
            }
            else if (player.HasAuxiliaryAbility)
            {
               player.DeactivateAuxiliaryAbility();
            }

            //Handheld Primary interaction
            if (!isBusy && player.HasHandheld && player.HandheldItem.CanActivatePrimary() && ((Utilities.HasFlag(player.HandheldItem.PrimaryInputType,InputType.Hold) && Input.GetButton("Primary")) || (Utilities.HasFlag(player.HandheldItem.PrimaryInputType, InputType.Press) && Input.GetButtonDown("Primary"))))
            {
                player.ActivateHandheldPrimary();
                isBusy = true;
            }
            else if(!isBusy && player.HasHandheld)
            {
                player.DeactivateHandheldPrimary();
            }

            //Handheld Secondary interaction
            if (!isBusy & player.HasHandheld  && player.HandheldItem.CanActivateSecondary() && ((Utilities.HasFlag(player.HandheldItem.SecondaryInputType,InputType.Hold) && Input.GetButton("Secondary")) || (Utilities.HasFlag(player.HandheldItem.SecondaryInputType,InputType.Press) && Input.GetButtonDown("Secondary"))))
            {
                player.ActivateHandheldSecondary();
                isBusy = true;
            }
            else if(!isBusy && player.HasHandheld)
            {
                player.DeactivateHandheldSecondary();
            }

            //Handheld Tertiary interaction
            if (!isBusy & player.HasHandheld && player.HandheldItem.CanActivateTertiary() && ((Utilities.HasFlag(player.HandheldItem.TertiaryInputType, InputType.Hold) && Input.GetButton("Tertiary")) || (Utilities.HasFlag(player.HandheldItem.TertiaryInputType,InputType.Press) && Input.GetButtonDown("Tertiary"))))
            {
                player.ActivateHandheldTertiary();
                isBusy = true;
            }
            else if(!isBusy && player.HasHandheld)
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
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = Vector3.zero;
        Vector3 aimPoint = m_Transform.position;
        RaycastHit hit;

        // Create a ray from the mouse cursor on screen in the direction of the camera.
        Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);


        if (Physics.Raycast(camRay, out hit, MAX_DISTANCE, hitMask))
        {
            aimPoint = hit.point;
        }
        else
        {

            Plane plane = new Plane(Vector3.up, Vector3.zero);

            //Debug.DrawRay (camRay.origin, camRay.direction * 100f, Color.green);


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
        //new Vector3(Camera.main.transform.right * h, 0, Camera.main.transform.up * v);  //new Vector3(h, 0, v); 

        aimPoint.y = m_Transform.position.y;

        player.HandleInput(moveDir, aimPoint);


    }





    public bool CanAttack
    {
		get { return canAttack; }
		set { canAttack = value; }
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
