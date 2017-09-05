using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class InvestigateNoise : BaseUtilityBehavior
{


    public override void StartBehavior()
    {
        throw new NotImplementedException();
    }
    public override void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        throw new NotImplementedException();
    }



    public override float GetBehaviorScore()
    {
        throw new NotImplementedException();
    }




    public override bool CanStartSubBehavior
    {
        get { return true; }
    }
    public override bool CanEndBehavior
    {
        get { return true; }
    }


  

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }
    



    public override string ToString()
    {
        return "Investigate Noise";
    }
}
