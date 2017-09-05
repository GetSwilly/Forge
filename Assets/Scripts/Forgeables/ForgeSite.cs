using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForgeSite : MonoBehaviour {

    [SerializeField]
    List<GameObject> m_PossibleForges = new List<GameObject>();


    Transform m_Transform;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }


    
    public void Forge(GameObject obj)
    {
        IForgeable fScript = obj.GetComponent<IForgeable>();

        if (fScript == null)
            return;
    }
}
