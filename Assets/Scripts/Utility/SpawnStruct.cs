using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnStruct {

    [SerializeField]
    Vector3 localOffset;

    [SerializeField]
    Vector3 spawnArea;

    [SerializeField]
    float launchPower;


    public SpawnStruct(Vector3 _offset, Vector3 _area, float _power)
    {
        localOffset = _offset;
        spawnArea = _area;
        launchPower = _power;
    }




    public Vector3 LocalOffset
    {
        get { return localOffset; }
        set { localOffset = value; }
    }
    public Vector3 SpawnArea
    {
        get { return spawnArea; }
        set { spawnArea = value; }
    }
    public float LaunchPower
    {
        get { return launchPower; }
        set { launchPower = value; }
    }
}
