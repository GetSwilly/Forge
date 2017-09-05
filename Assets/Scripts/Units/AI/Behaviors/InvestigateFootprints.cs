using UnityEngine;
using System.Collections;
using System;

public class InvestigateFootprints : BaseUtilityBehavior
{
    //Footprint curFootprint = null;



    public override void StartBehavior()
    {
        throw new NotImplementedException();
    }
   
    public override bool CanStartSubBehavior
    {
        get { return true; }
    }


    public override float GetBehaviorScore()
    {
        throw new NotImplementedException();
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }

   

    public override string ToString()
    {
        return "Investigate Footprints";
    }
}
