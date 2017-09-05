using UnityEngine;
using System.Collections;

public class Footprint : MonoBehaviour {

    static readonly float MAX_VALUE = 100f;


    float currentValue = 0f;
    Vector3 printDirection = Vector3.zero;

    [SerializeField]
    [Range(0f, 100f)]
    protected float decayTime;

    Transform ownerTransform = null;


    void OnEnable()
    {
        currentValue = MAX_VALUE;
    }
    

    void Update()
    {
        ChangeValue(-1f / decayTime);
    }



    void ChangeValue(float delta)
    {
        currentValue = Mathf.Clamp(currentValue + delta, 0, MAX_VALUE);
    }



    public float CurrentValue
    {
        get { return currentValue; }
    }
    public Vector3 Direction
    {
        get { return printDirection; }
    }
    public Vector3 Position
    {
        get { return transform.position; }
    }
    

    public Transform Owner
    {
        get { return ownerTransform; }
    }
}
