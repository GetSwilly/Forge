using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Orientation
{

    [SerializeField]
    Vector3 position;

    [SerializeField]
    Vector3 euler;

    [SerializeField]
    Vector3 scale;

    public Orientation(Vector3 _position, Vector3 _euler, Vector3 _scale)
    {
        position = _position;
        euler = _euler;
        scale = _scale;
    }


    public Vector3 LocalPosition
    {
        get { return position; }
        set { position = value; }
    }
    public Vector3 LocalEuler
    {
        get { return euler; }
        set { euler = value; }
    }
    public Vector3 LocalScale
    {
        get { return scale; }
        set { scale = value; }
    }
}
