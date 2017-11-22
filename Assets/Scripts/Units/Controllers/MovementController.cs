using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementController : MonoBehaviour, IMovement
{
    //static readonly float GROUND_CHECK_DISTANCE = .6f;
    static readonly float SLOPE_CHECK_HEIGHT = 0.05f;

    [Tooltip("Should show debug information?")]
    [SerializeField]
    bool showDebug = false;


    [Tooltip("Speed of movement")]
    [SerializeField]
    float movementSpeed;

    float additionalSpeedMultiplier = 0f;


    //Move with different speed based on direction of movement
    [Tooltip("Ratio applied to speed based on direction of travel in relation of angle to directly forward.")]
    [SerializeField]
    AnimationCurve directionalSpeedupCurve = AnimationCurve.Linear(0f, 1f, 180f, .5f);

    [Tooltip("Speed at which object is capable of rotating.")]
    [SerializeField]
    float rotationSpeed = 5f;

    //[SerializeField]
    //LayerMask groundMask;


    protected Transform m_Transform;
    CharacterController m_Character;
    Rigidbody m_Rigidbody;

    Dictionary<object, float> speedMultiplierSet = new Dictionary<object, float>();
    float speedMultiplier = 1f;

    Dictionary<object, float> rotationMultiplierSet = new Dictionary<object, float>();
    float rotationMultiplier = 1f;


    Vector3 bounds;
    float maxBoundsValue;


    public virtual void Awake()
    {
        m_Transform = this.GetComponent<Transform>();
        m_Rigidbody = this.GetComponent<Rigidbody>();
        m_Character = GetComponent<CharacterController>();

        // speedValue = (float)Utilities.GetRandomGaussian(m_Speed);
    }
    void Start()
    {
        bounds = Utilities.CalculateObjectBounds(gameObject, false);
        maxBoundsValue = Mathf.Max(bounds.x, bounds.z);
    }

    public virtual void Move(Vector3 moveDir)
    {
        if (!this.enabled)
            return;


        if (moveDir.magnitude == 0)
            return;

       // Vector3 originPos = m_Transform.position + (Vector3.up * SLOPE_CHECK_HEIGHT);

        //if (showDebug)
        //{
        //    Debug.DrawRay(originPos, moveDir.normalized * GROUND_CHECK_DISTANCE, Color.red);
        //}

        if (moveDir.magnitude > 1)
            moveDir.Normalize();

        if (showDebug)
            Debug.DrawLine(m_Transform.position, m_Transform.position + (moveDir * 3f), Color.cyan);




        Vector3 velVector = m_Rigidbody.velocity;
        velVector.y = 0;
        velVector.Normalize();

        float dirSpeedup = directionalSpeedupCurve.Evaluate(Vector3.Angle(m_Transform.forward, moveDir));

        Vector3 forceVector = moveDir.normalized * Speed * dirSpeedup * Time.deltaTime;

        m_Rigidbody.AddForce(forceVector, ForceMode.Acceleration);
    }
    public void MoveToPosition(Vector3 position)
    {
        Vector3 movementVector = position - m_Transform.position;

        if (movementVector.magnitude > (Speed * Time.deltaTime))
        {
            Debug.Log(movementVector.magnitude);
            movementVector = movementVector.normalized * Speed * Time.deltaTime;
            position = m_Transform.position + movementVector;
            Debug.Log(movementVector.magnitude);
        }

        if (m_Character != null && m_Character.enabled)
        {
            m_Character.Move(movementVector);
            position = m_Transform.position;
        }

        if (m_Rigidbody != null)
        {
            m_Rigidbody.MovePosition(position);
        }
        else
        {
            m_Transform.position = position;
        }
    }

    public void RotateTowards(Transform t)
    {
        RotateTowards(t, 1f);
    }
    public void RotateTowards(Transform t, float _multiplier)
    {
        if (t != null)
            RotateTowards(t.position, _multiplier);
    }
    public void RotateTowards(Vector3 lookAtPosition)
    {
        RotateTowards(lookAtPosition, 1f);
    }
    public void RotateTowards(Vector3 lookAtPosition, float _multiplier)
    {
        if (!this.enabled)
            return;


        Vector3 direction = lookAtPosition - m_Transform.position;
        direction.y = 0;
        /*
        if (showDebug)
        {
            Debug.Log(string.Format("Rotate towards {0}. Distance {1}.", lookAtPosition, dispVector.magnitude));
            Debug.DrawLine(myTransform.position, lookAtPosition, Color.yellow);
        }
        */

        if (direction.magnitude <= maxBoundsValue)
            return;

        //Quaternion desiredQ = Quaternion.LookRotation(Vector3.forward, lookAtPosition - myTransform.position);

        //myTransform.rotation =  Quaternion.Slerp(myTransform.rotation, desiredQ, rotationSmoothing);

        Quaternion desiredQ = Quaternion.LookRotation(direction);

        m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, desiredQ, RotationSpeed * _multiplier);// * Time.deltaTime);
    }


    public void MoveTowards(Vector3 targetPosition)
    {
        RotateTowards(targetPosition);
        Move(targetPosition - m_Transform.position);
    }

    public void Move(Vector3 position3D, Vector3 deltaPosition, bool useDeltaAsDirection)
    {
        Move(position3D, deltaPosition, useDeltaAsDirection, Speed);
    }
    public void Move(Vector3 position3D, Vector3 deltaPosition, bool useDeltaAsDirection, float speed)
    {
        if (m_Character != null && m_Character.enabled)
        {
            if (useDeltaAsDirection)
            {
                deltaPosition = deltaPosition.normalized * speed * Time.deltaTime;
            }


            // Use CharacterController
            m_Transform.position = position3D;
            m_Character.Move(deltaPosition);
            // Grab the position after the movement to be able to take physics into account
            // TODO: Add this into the clampedPosition calculation below to make RVO better respond to physics
            position3D = m_Transform.position;
        }

        if (m_Rigidbody != null)
        {
            m_Rigidbody.MovePosition(position3D);
        }
        else
        {
            m_Transform.position = position3D;
        }
    }
    public virtual void RotateTowards(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, m_Transform.up);

        m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, targetRotation, RotationSpeed);// * Time.deltaTime);
    }


    #region Modifiers

    public void AddSpeedMultiplier(object obj, float multiplier)
    {
        if (Mathf.Approximately(multiplier, 1f))
            return;

        if (speedMultiplierSet.ContainsKey(obj))
        {
            speedMultiplierSet[obj] = multiplier;
        }
        else
        {
            speedMultiplierSet.Add(obj, multiplier);
        }

        speedMultiplier = GetTotalSpeedMultiplier();
    }
    public void RemoveSpeedMultiplier(object obj)
    {
        if (!speedMultiplierSet.ContainsKey(obj))
        {
            return;
        }

        speedMultiplierSet.Remove(obj);

        speedMultiplier = GetTotalSpeedMultiplier();
    }
    float GetTotalSpeedMultiplier()
    {
        Dictionary<object, float>.Enumerator enumerator = speedMultiplierSet.GetEnumerator();

        float multiplier = 1f;

        while (enumerator.MoveNext())
        {
            multiplier *= enumerator.Current.Value;
        }

        return multiplier;
    }



    public void AddRotationMultiplier(object obj, float multiplier)
    {
        if (Mathf.Approximately(multiplier, 1f))
            return;

        if (rotationMultiplierSet.ContainsKey(obj))
        {
            rotationMultiplierSet[obj] = multiplier;
        }
        else
        {
            rotationMultiplierSet.Add(obj, multiplier);
        }

        rotationMultiplier = GetTotalRotationMultiplier();
    }
    public void RemoveRotationMultiplier(object obj)
    {
        if (!rotationMultiplierSet.ContainsKey(obj))
        {
            return;
        }

        rotationMultiplierSet.Remove(obj);

        rotationMultiplier = GetTotalRotationMultiplier();
    }
    float GetTotalRotationMultiplier()
    {
        Dictionary<object, float>.Enumerator enumerator = rotationMultiplierSet.GetEnumerator();

        float multiplier = 1f;

        while (enumerator.MoveNext())
        {
            multiplier *= enumerator.Current.Value;
        }

        return multiplier;
    }

    #endregion

    #region Accessors

    public float Speed
    {
        get
        {
            return movementSpeed * (1f + AdditionalSpeedMultiplier) * MovementSpeedMultiplier;
        }
        set { movementSpeed = Mathf.Clamp(value, 0f, value); }
    }
    public float MovementSpeedMultiplier
    {
        get { return speedMultiplier; }
        set { speedMultiplier = value; }
    }


    public float RotationSpeed
    {
        get { return rotationSpeed * RotationMultiplier; }
        set { rotationSpeed = Mathf.Clamp(value, 0f, value); }
    }
    public float RotationMultiplier
    {
        get { return rotationMultiplier; }
        set { rotationMultiplier = value; }
    }

    public float AdditionalSpeedMultiplier
    {
        get { return additionalSpeedMultiplier; }
        set { additionalSpeedMultiplier = value; }
    }
    public Vector3 Velocity
    {
        get
        {
            if (m_Rigidbody != null)
                return m_Rigidbody.velocity;

            return Vector3.zero;
        }
    }

    #endregion




    void OnValidate()
    {
        Speed = Speed;
        RotationSpeed = RotationSpeed;

        Utilities.ValidateCurve_Times(directionalSpeedupCurve, 0f, 180f);
    }


    void OnDrawGizmos()
    {
        if (showDebug && m_Transform != null)
        {
            Vector3 vel = Velocity;
            vel.y = 0;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_Transform.position, m_Transform.position + vel);
        }
    }
}
