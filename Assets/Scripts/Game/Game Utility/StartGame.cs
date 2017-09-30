using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartGame : InteractableObject
{

    public override bool Interact(PlayerController player)
    {

        if (activatingObjects.Count == 0)
            return false;



        PlaySound(activationSound);

        GameManager.Instance.StartGame(player);
        //GameManager.Instance.NextLevel();

        OnUseTrigger();

        return true;
    }

    public override void Drop()
    {

    }



    public override bool IsUsableOutsideFOV
    {
        get { return true; }
    }
}

