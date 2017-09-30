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

    public override bool Interact(PlayerController _player)
    {
        pController = _player;

        switch (myType)
        {
            case PickupType.Handheld:
                _player.Pickup(m_Handheld);
                break;
            case PickupType.Ability:
                _player.Pickup(m_Ability);
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
