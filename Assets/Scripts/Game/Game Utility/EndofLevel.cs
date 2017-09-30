using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EndofLevel : InteractableObject
{

    public override bool Interact(PlayerController player)
    {

        if (activatingObjects.Count == 0)
            return false;


        GameManager.Instance.EndLevel();


        OnUseTrigger();

        return true;
    }

    public override void Drop()
    {
        throw new NotImplementedException();
    }

    public override bool IsUsableOutsideFOV
    { 
		get { return true; }
	}
}
