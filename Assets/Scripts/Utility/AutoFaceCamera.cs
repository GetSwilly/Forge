using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFaceCamera : MonoBehaviour
{

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
            m_Transform.rotation = Camera.main.transform.rotation;
        }
    }


    public float RotationSpeed
    {
        get { return m_RotationSpeed; }
        set
        {
            m_RotationSpeed = value;

            if (m_RotationSpeed < 0f)
                m_RotationSpeed = 0f;
        }
    }




    void OnValidate()
    {
        RotationSpeed = RotationSpeed;
    }
}
