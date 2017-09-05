using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Animation))]
public class Taunt : BaseUtilityBehavior
{
    [SerializeField]
    AnimationClip myAnimationClip;


    Animation myAnim;



    public override void Awake()
    {
        base.Awake();

        myAnim = GetComponent<Animation>();
    }

    IEnumerator PlayAnimation()
    {
        myAnim.Play("Taunt");
        yield return myAnimationClip.length;
        EndBehavior(true, true);
    }
    public override void StartBehavior()
    {
        IsActive = true;

        StartCoroutine(PlayAnimation());
    }



    public override bool CanStartSubBehavior
    {
       get { throw new NotImplementedException(); }
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
        return "Taunt";
    }
}
