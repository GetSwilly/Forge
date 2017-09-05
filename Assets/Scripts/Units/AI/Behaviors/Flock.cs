using UnityEngine;
using System.Collections;
using System;

public class Flock : BaseUtilityBehavior {

    [SerializeField]
    [Range(0f, 1f)]
    float alignmentStrength = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    float cohesionStrength = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    float separationStrength = 1f;

    [SerializeField]
    int desiredAllyCount = 3;


    MovementController myMovement;

    public override void Awake()
    {
        base.Awake();

        myMovement = GetComponent<MovementController>();
    }



    IEnumerator MaintainFlock()
    {
        while (IsActive)
        {

            float totalStrength = alignmentStrength + cohesionStrength + separationStrength;

            Vector3 alignVector = Alignment() * (alignmentStrength / totalStrength);
            Vector3 cohesionVector = Cohesion() * (cohesionStrength / totalStrength);
            Vector3 separationVector = Separation() * (separationStrength / totalStrength);

            Vector3 aggregateVector = alignVector + cohesionVector + separationVector;
            aggregateVector.y = 0;

            Vector3 curVel = myMovement.Velocity;
            curVel.y = 0;

            Vector3 newVel = (curVel + aggregateVector).normalized * curVel.magnitude;

            Rigidbody _rigid = GetComponent<Rigidbody>();
            _rigid.velocity = newVel;

            yield return null;
        }
    }

    Vector3 Alignment()
    {
        throw new NotImplementedException();
    }
    Vector3 Cohesion()
    {
        throw new NotImplementedException();
    }
    Vector3 Separation()
    {
        throw new NotImplementedException();
    }


    public override void StartBehavior()
    {
        IsActive = true;
    }


    public override bool CanStartSubBehavior
    {
        get { return true; }
    }

    public override float GetBehaviorScore()
    {
        float count = m_Actor.NearbyAllies.Count;

        if (count == 0)
            return 0f;

        return utilityCurve.Evaluate(Mathf.Clamp01(count / desiredAllyCount));
    }

    public override void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior)
    {
        throw new NotImplementedException();
    }



    public override string ToString()
    {
        return "Flock";
    }
    


    void OnValidate()
    {
        desiredAllyCount = Mathf.Max(desiredAllyCount, 1);
    }
}
