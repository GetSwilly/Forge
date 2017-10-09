using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AOETurret : ForgeableObject
{

    [SerializeField]
    float range;

    public override void Initialize(ForgeSite activator, Team team)
    {
        throw new System.NotImplementedException();
    }

    public float Range
    {
        get { return range; }
        protected set { range = Mathf.Clamp(value, 0f, value); }
    }

    protected virtual void OnValidate()
    {
        Range = Range;
    }
}
