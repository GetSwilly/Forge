using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathDrop : MonoBehaviour {

    [SerializeField]
    ListDefinitionName itemPool = ListDefinitionName.GeneralItems;

    [SerializeField]
    int numberOfDrops = 10;

    Health m_Health;

    void Awake()
    {
        m_Health = GetComponent<Health>();
    }
	void Start () {
        m_Health.OnKilled += Drop;	
	}

    void Drop(Health h)
    {
        if (!this.enabled || GameManager.Instance == null)
            return;

        GameManager.Instance.DropItems(this.transform, itemPool, NumberOfDrops);
    }

    protected int NumberOfDrops
    {
        get { return numberOfDrops; }
        private set { numberOfDrops = Mathf.Clamp(value, 0, value); }
    }
    

    void OnValidate()
    {
        NumberOfDrops = NumberOfDrops;
    }
}
