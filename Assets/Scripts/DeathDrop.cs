using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathDrop : MonoBehaviour {

    [SerializeField]
    int numberOfDrops = 10;

    Health m_Health;

    void Awake()
    {
        m_Health = GetComponent<Health>();
    }
	void Start () {
        //m_Health.OnKilled += Drop;	
	}

    public int NumberOfDrops
    {
        get { return numberOfDrops; }
        private set { numberOfDrops = Mathf.Clamp(value, 0, value); }
    }
    

    void OnValidate()
    {
        NumberOfDrops = NumberOfDrops;
    }
}
