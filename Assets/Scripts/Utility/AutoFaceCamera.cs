using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFaceCamera : MonoBehaviour
{
    [SerializeField]
    RotationType m_RotationType;

    [SerializeField]
    float m_RotationSpeed = 1f;


    Transform m_Transform;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }


    void Update()
    {
        if (Camera.main != null)
        {
            switch (m_RotationType)
            {
                case RotationType.Slerp:
                    m_Transform.rotation = Quaternion.Slerp(m_Transform.localRotation, Camera.main.transform.rotation, RotationSpeed * Time.deltaTime);
                    break;
                case RotationType.RotateTowards:
                    m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, Camera.main.transform.rotation, RotationSpeed * Time.deltaTime);
                    break;
            }

            //m_Transform.rotation = Camera.main.transform.rotation;
        }
    }


    public float RotationSpeed
    {
        get { return m_RotationSpeed; }
        set { m_RotationSpeed = Mathf.Clamp(value, 0f, value); }
    }




    void OnValidate()
    {
        RotationSpeed = RotationSpeed;
    }
}
