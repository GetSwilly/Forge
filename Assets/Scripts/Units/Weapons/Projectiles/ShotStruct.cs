using UnityEngine;
using System.Collections;

[System.Serializable]
public class Shot
{
    [SerializeField]
    [Range(0f, 2f)]
    float powerModifier = 1f;

    [SerializeField]
    [Range(0f,3f)]
    float speedModifier = 1f;

    [SerializeField]
    [Range(0f, 3f)]
    float rangeModifier = 1f;


    [SerializeField]
    Vector3 localOffset = new Vector3(0, 0, 1);

    [SerializeField]
    Vector3 localDirection = new Vector3(0, 0, 1);

    [SerializeField]
    float delay = 0f;

    [SerializeField]
    float ammoCost = 0;
    



    public float PowerModifier
    {
        get { return powerModifier; }
    }

    public float SpeedModifier
    {
        get { return speedModifier; }
    }

    public float RangeModifier
    {
        get { return rangeModifier; }
    }

    public Vector3 LocalOffset
    {
        get { return localOffset; }
    }

    public Vector3 LocalDirection
    {
        get { return localDirection; }
    }

    public float Delay
    {
        get { return delay; }
        set { delay = Mathf.Clamp(value, 0f, delay); }
    }

    public float AmmoCost
    {
        get { return ammoCost; }
        set { ammoCost = value; }
    }

    public void Validate()
    {
        Delay = Delay;
    }
}
