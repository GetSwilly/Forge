using UnityEngine;
using System.Collections;
using System;

public class ItemPickup : InteractableObject
{

    public enum PickupType { Handheld, Ability };

    [SerializeField]
    PickupType myType;

    PlayerController pController;
    HandheldItem m_Handheld = null;
    Ability m_Ability = null;

    protected override void Awake()
    {
        base.Awake();

        switch (myType)
        {
            case PickupType.Handheld:
                m_Handheld = GetComponent<HandheldItem>();
                break;
            case PickupType.Ability:
                m_Ability = GetComponent<Ability>();
                break;
        }

        Name = myType == PickupType.Handheld ? m_Handheld.ToString() : m_Ability.ToString();
    }
    void OnDisable()
    {
        Debug.Log("DISABLING");
    }
    public override bool Interact(PlayerController player)
    {
        pController = player;

        switch (myType)
        {
            case PickupType.Handheld:
                player.Pickup(m_Handheld);
                break;
            case PickupType.Ability:
                player.Pickup(m_Ability);
                break;
        }

        OnUseTrigger();
        return true;
    }




    public override void Drop()
    {
        if(pController != null)
        {
            pController.DropHandheld(GetComponent<HandheldItem>());
        }
    }


    public override bool IsUsableOutsideFOV
    {
        get { return false; }
    }
    
}
