using SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class SightedObject
{
    private Transform sightedTransform;
    private Vector3 lastKnownBasePosition;
    private List<Vector3> lastKnownTargetPositions;
    private Vector3 lastKnownDirection;
    private float lastTimeSeen;
    private bool inSight;

    public SightedObject(Transform _transform)
    {
        sightedTransform = _transform;
    }
    public SightedObject(Transform _transform, Vector3 basePos, List<Vector3> _pos, Vector3 _dir, bool _sight)
    {
        sightedTransform = _transform;
        lastKnownBasePosition = basePos;
        lastKnownTargetPositions = _pos;
        lastKnownDirection = _dir;
        lastTimeSeen = Time.time;
        inSight = _sight;

    }


    public void UpdatePositions()
    {
        InSight = true;

        LastKnownBasePosition = sightedTransform.position;

        List<LOSTargets> losTargets = sightedTransform.GetComponentsInChildren<LOSTargets>().ToList();
        List<Vector3> losPositions = new List<Vector3>();
        losTargets.ForEach(t => losPositions.Add(t.transform.position));

        LastKnownPositions = losPositions;

        LastTimeSeen = Time.time;

        Rigidbody _rigid = sightedTransform.GetComponent<Rigidbody>();
        if (_rigid != null)
        {
            LastKnownDirection = _rigid.velocity;
        }
    }

    public Transform SightedTransform
    {
        get { return sightedTransform; }
    }
    public Vector3 LastKnownBasePosition
    {
        get { return lastKnownBasePosition; }
        set { lastKnownBasePosition = value; }
    }
    public List<Vector3> LastKnownPositions
    {
        get { return lastKnownTargetPositions; }
        set { lastKnownTargetPositions = value; }
    }
    public Vector3 LastKnownDirection
    {
        get { return lastKnownDirection; }
        set { lastKnownDirection = value; }
    }
    public float LastTimeSeen
    {
        get { return lastTimeSeen; }
        set { lastTimeSeen = value; }
    }
    public bool InSight
    {
        get { return inSight; }
        set { inSight = value; }
    }



    public override string ToString()
    {
        return string.Format("Sighted Transform: {0}. Last Known Base Position: {1}. Last Known Direction: {2}. Last Time Seen: {3}. In Sight: {4}", SightedTransform, LastKnownBasePosition, LastKnownDirection, LastTimeSeen, InSight);
    }
}
