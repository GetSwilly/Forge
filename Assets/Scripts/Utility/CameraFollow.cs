using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {


    class VariableStorage
    {
        FollowStyle m_Style;
        Transform m_TargetTransform;
        Vector3 m_TargetPosition;
        Vector3 m_CameraOffset;
        float m_DesiredAngle;
        float m_AimOffset;
        float m_MoveSpeed;
        float m_RotationSpeed;
        float m_IgnoreAngle;


       public VariableStorage(FollowStyle _style, Transform _targetTransform, Vector3 _targetPosition, Vector3 _offset, float _desiredAngle, float _aimOffset, float _moveSpeed, float _rotationSpeed, float _ignoreAngle)
        {
            m_Style = _style;
            m_TargetTransform = _targetTransform;
            m_TargetPosition = _targetPosition;
            m_CameraOffset = _offset;
            m_DesiredAngle = _desiredAngle;
            m_AimOffset = _aimOffset;
            m_MoveSpeed = _moveSpeed;
            m_RotationSpeed = _rotationSpeed;
            m_IgnoreAngle = _ignoreAngle;
        }

        public FollowStyle Style
        {
            get { return m_Style; }
        }
        public Transform TargetTransform
        {
            get { return m_TargetTransform; }
        }
        public Vector3 TargetPosition
        {
            get { return m_TargetPosition; }
        }
        public Vector3 CameraOffset
        {
            get { return m_CameraOffset; }
        }
        public float DesiredAngle
        {
            get { return m_DesiredAngle; }
        }
        public float AimOffset
        {
            get { return m_AimOffset; }
        }
        public float MoveSpeed
        {
            get { return m_MoveSpeed; }
        }
        public float RotationSpeed
        {
            get { return m_RotationSpeed; }
        }
        public float IgnoreAngle
        {
            get { return m_IgnoreAngle; }
        }
    }



    public enum FollowStyle { Target, Position };

    [SerializeField]
    FollowStyle m_FollowStyle = FollowStyle.Target;


    [SerializeField]
    Transform targetTransform;
    Vector3 targetPosition;


    [SerializeField]
    Vector3 m_CameraOffset = new Vector3(0f, 50f, -10f);

    [SerializeField]
    [Range(0f, 90f)]
    float m_DesiredAngle = 45f;

    [SerializeField]
    float m_AimOffset = 2f;

    [SerializeField]
    float m_MoveSpeed = 0.7f;

    [SerializeField]
    float m_RotationSpeed = 0f;

    [SerializeField]
    [Range(0f, 180f)]
    float m_IgnoreAngle = 5f;


    Vector3 assignedPosition = Vector3.zero;
    Vector3 desiredPosition = Vector3.zero;
    Transform m_Transform;
    VariableStorage m_StoredVariables = null;

    void Awake()
    {
        m_Transform = this.GetComponent<Transform>();
    }
    
    void LateUpdate()
    {
        if (m_FollowStyle == FollowStyle.Target)
        {
            if (TargetTransform == null)
                return;


            Vector3 lookAtPosition = TargetTransform.position + TargetTransform.forward;
            lookAtPosition.y = 0;

            Vector3 aimDir = Vector3.zero;


            Vector3 lookAngle = lookAtPosition - m_Transform.position;
            lookAngle.y = 0;

            //if (Vector3.Angle(myTransform.up, targetTransform.forward) > ignoreAngle)
            //{
            //    Quaternion desiredQ = Quaternion.LookRotation(Vector3.down, targetTransform.forward);
            //   // myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, desiredQ, rotationSpeed);
            //}



            m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, Quaternion.AngleAxis(DesiredAngle, m_Transform.right), RotationSpeed * Time.deltaTime);


            aimDir = TargetTransform.forward;
            aimDir.y = 0;
            aimDir.Normalize();


            assignedPosition = TargetTransform.position + CameraOffset + (aimDir * AimOffset);
        }
        else if (m_FollowStyle == FollowStyle.Position)
        {
            m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, Quaternion.AngleAxis(DesiredAngle, m_Transform.right), RotationSpeed * Time.deltaTime);

            assignedPosition = TargetPosition;
        }

        desiredPosition = Vector3.Lerp(m_Transform.position, assignedPosition, MoveSpeed * Time.deltaTime);

        m_Transform.position = DesiredPosition;
    }



    public void StoreVariables()
    {
        m_StoredVariables = new VariableStorage(CurrentFollowStyle, TargetTransform, TargetPosition, CameraOffset, DesiredAngle, AimOffset, MoveSpeed, RotationSpeed, IgnoreAngle);
    }
    public void ResetVariables()
    {
        if (m_StoredVariables == null)
            return;

        CurrentFollowStyle = m_StoredVariables.Style;
        AimOffset = m_StoredVariables.AimOffset;
        CameraOffset = m_StoredVariables.CameraOffset;
        DesiredAngle = m_StoredVariables.DesiredAngle;
        IgnoreAngle = m_StoredVariables.IgnoreAngle;
        MoveSpeed = m_StoredVariables.MoveSpeed;
        RotationSpeed = m_StoredVariables.RotationSpeed;
        TargetPosition = m_StoredVariables.TargetPosition;
        TargetTransform = m_StoredVariables.TargetTransform;

        m_StoredVariables = null;
    }





    public FollowStyle CurrentFollowStyle
    {
        get { return m_FollowStyle;}
        set { m_FollowStyle = value; }
    }
    public Transform TargetTransform
    {
        get { return targetTransform; }
        set { targetTransform = value; }
    }
    public Vector3 TargetPosition
    {
        get { return targetPosition; }
        set { targetPosition = value; }
    }
	public Vector3 DesiredPosition
    {
		get { return desiredPosition; }
	}
    
    public Vector3 AssignedPosition
    {
        get { return assignedPosition; }
        set { assignedPosition = value; }
    }
    public Vector3 CameraOffset
    {
        get { return m_CameraOffset; }
        set { m_CameraOffset = value; }
    }
    public float AimOffset
    {
        get { return m_AimOffset; }
        set { m_AimOffset = value; }
    }
    public float DesiredAngle
    {
        get { return m_DesiredAngle; }
        set { m_DesiredAngle = value; }
    }
	public float MoveSpeed
    {
		get { return m_MoveSpeed; }
		set { m_MoveSpeed = value; }
	}
    public float RotationSpeed
    {
        get { return m_RotationSpeed; }
        set { m_RotationSpeed = value; }
    }

    public float IgnoreAngle
    {
        get { return m_IgnoreAngle; }
        set { m_IgnoreAngle = value; }
    }
}
