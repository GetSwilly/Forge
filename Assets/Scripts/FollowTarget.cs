using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour {

    static readonly float SNAP_DISTANCE = 0.1f;
    static readonly float IGNORE_MOVEMENT_DISTANCE = 0.1f;

    [SerializeField]
    Transform targetTransform;

    [SerializeField]
    Vector3 targetOffset = Vector3.zero;



    [SerializeField]
    float movementSpeed;

    [SerializeField]
    float rotationSpeed;

    [SerializeField]
    [Range(0f, 1f)]
    float speedDecay = 0.1f;

    Vector3 velocity = Vector3.zero;


    public enum RotationType { NONE, SEMI, FULL };
    public RotationType myRotationType = RotationType.NONE;



    [SerializeField]
    bool disableIfNoTarget = true;

	Vector3 desiredPos = Vector3.zero;
	Transform m_Transform;
	
	void Awake ()
    {
		m_Transform = this.GetComponent<Transform> ();
	}
	


	void LateUpdate ()
    {
        if (targetTransform == null)
        {
            if (disableIfNoTarget)
            {
                gameObject.SetActive(false);
            }


            return;
        }


        velocity *= (1f - speedDecay) * (1f - Time.deltaTime);
        
        Move();

        Rotate();
	}
	
   
    void Move()
    {
       // if (targetTransform != null && targetTransform.gameObject.activeInHierarchy)
       // {
            desiredPos = targetTransform.position + targetOffset;
       // }

        Vector3 moveDir = desiredPos - m_Transform.position;

       // if (moveDir.magnitude <= IGNORE_MOVEMENT_DISTANCE)
     //       return;


        Vector3 moveVector = moveDir.normalized * MovementSpeed * Time.deltaTime;

        

        if(moveVector.magnitude > moveDir.magnitude)
        {
            moveVector = moveVector.normalized * moveDir.magnitude;
        }

        velocity += moveVector;

        m_Transform.position += velocity;
    }
    void Rotate()
    {
        Quaternion desiredQ;

        switch (myRotationType)
        {
            case RotationType.SEMI:
                desiredQ = Quaternion.Euler(targetTransform.eulerAngles.x, 0, targetTransform.eulerAngles.z);
                break;
            case RotationType.FULL:
                desiredQ = targetTransform.rotation;
                break;
            default:
                desiredQ = m_Transform.rotation;
                break;
        }

        m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, desiredQ, RotationSpeed);

    }




    public float MovementSpeed
    {
        get { return movementSpeed; }
        private set { movementSpeed = Mathf.Clamp(value, 0f, value); }
    }
    public float RotationSpeed
    {
        get { return rotationSpeed; }
        private set { rotationSpeed = Mathf.Clamp(value, 0f, value); }
    }


    public Transform TargetTransform
    {
        get { return targetTransform; }
        set { targetTransform = value; }
    }
    public Vector3 TargetOffset
    {
        get { return targetOffset; }
        set
        {
            targetOffset = value;
        }
    }
    
	public Vector3 DesiredPosition
    {
		get { return desiredPos; }
	}




    void OnValidate()
    {
        MovementSpeed = MovementSpeed;
        RotationSpeed = RotationSpeed;
    }
}
