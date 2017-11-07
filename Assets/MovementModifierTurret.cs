using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementModifierTurret : ForgeableObject
{

    [SerializeField]
    float movementModifier;

    [SerializeField]
    float rotationModifier;

    protected override void SightGained(GameObject obj)
    {
        base.SightGained(obj);

        IMovement movement = obj.GetComponent<IMovement>();
        Team team = obj.GetComponent<Team>();
        if (movement != null && team != null && ShouldAffect(team))
        {
            movement.AddSpeedMultiplier(this, MovementModifier);
            movement.AddRotationMultiplier(this, RotationModifier);
        }
    }
    protected override void SightLost(GameObject obj)
    {
        base.SightGained(obj);

        IMovement movement = obj.GetComponent<IMovement>();
        if (movement != null)
        {
            movement.RemoveSpeedMultiplier(this);
            movement.RemoveRotationMultiplier(this);
        }
    }

    float MovementModifier
    {
        get { return movementModifier; }
        set { movementModifier = Mathf.Clamp(value, 0f, value); }
    }
    float RotationModifier
    {
        get { return rotationModifier; }
        set { rotationModifier = Mathf.Clamp(value, 0f, value); }
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        MovementModifier = MovementModifier;
        RotationModifier = RotationModifier;
    }
}
