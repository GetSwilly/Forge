using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour
{ 

    [SerializeField]
    Transform m_Target;

    [SerializeField]
    UpdateType m_UpdateType = UpdateType.None;

    [SerializeField]
    MovementType m_MovementType = MovementType.None;

    [SerializeField]
    RotationType m_RotationType;


    [SerializeField]
    float m_MoveSpeed = 10f;

    [SerializeField]
    float m_MoveSmoothTime = 1f;

    [SerializeField]
    float m_TurnSpeed = 10f;

    [SerializeField]
    float m_TurnSmoothing = 10f;

    private float m_LookAngle;
    private Vector3 movementVelocity = Vector3.zero;
    Transform m_Transform;

    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }
  

    protected virtual void Update()
    {
        if (m_UpdateType == UpdateType.Update) Follow(Time.fixedDeltaTime);
    }
    protected virtual void FixedUpdate()
    {
        if (m_UpdateType == UpdateType.FixedUpdate) Follow(Time.fixedDeltaTime);
    }

    protected virtual void LateUpdate()
    {
        if (m_UpdateType == UpdateType.LateUpdate) Follow(Time.deltaTime);
    }



    protected void Follow(float deltaTime)
    {
        FollowMovement(deltaTime);
        FollowRotation(deltaTime);
    }
    protected void FollowMovement(float deltaTime)
    {
        if (m_Target == null) return;

        switch (m_MovementType)
        {
            case MovementType.Lerp:
                m_Transform.position = Vector3.Lerp(m_Transform.position, m_Target.position, m_MoveSpeed * deltaTime);
                break;
            case MovementType.MoveTowards:
                m_Transform.position = Vector3.MoveTowards(m_Transform.position, m_Target.position, m_MoveSpeed * deltaTime);
                break;
            case MovementType.SmoothDamp:
                m_Transform.position = Vector3.SmoothDamp(m_Transform.position, m_Target.position, ref movementVelocity, m_MoveSmoothTime);
                break;
        }

    }
    public void FollowRotation(float deltaTime)
    {
        if (m_Target == null) return;

        switch (m_RotationType)
        {
            case RotationType.Slerp:
                m_Transform.rotation = Quaternion.Slerp(m_Transform.localRotation, m_Target.rotation, m_TurnSmoothing * deltaTime);
                break;
            case RotationType.RotateTowards:
                m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, m_Target.rotation, m_TurnSpeed * deltaTime);
                break;
        }
    }


    public virtual void SetTarget(Transform newTransform)
    {
        m_Target = newTransform;
    }



    #region Accessors

    public float MovementSpeed
    {
        get { return m_MoveSpeed; }
        set { m_MoveSpeed = Mathf.Clamp(value, 0f, value); }
    }
    public float MovementSmoothingTime
    {
        get { return m_MoveSmoothTime; }
        set { m_MoveSmoothTime = Mathf.Clamp(value, 0f, value); }
    }
    public float TurnSmoothing
    {
        get { return m_TurnSmoothing; }
        set { m_TurnSmoothing = Mathf.Clamp(value, 0f, value); }
    }

    public Transform Target
    {
        get { return m_Target; }
    }

    #endregion


    void OnValidate()
    {
        MovementSpeed = MovementSpeed;
        MovementSmoothingTime = MovementSmoothingTime;

        TurnSmoothing = TurnSmoothing;
    }
}
