using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGoal : DisplayUI {

    [SerializeField]
    Transform m_Target;
    

	void Update ()
    {
        if (m_Target == null || !m_Target.gameObject.activeInHierarchy)
        { 
            return;
        }
        
        Vector3 toVector = m_Target.position - m_Transform.position;
        toVector.y = 0;

        m_Transform.rotation = Quaternion.LookRotation(Vector3.down, toVector);
    }


    public Transform Target
    {
        set { m_Target = value; }
    }
}
