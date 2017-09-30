using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAIPath : AIPath, IPathfinder
{
    enum TargetType
    {
        Transform,
        Position
    }

    [SerializeField]
    TargetType m_Type;

    [SerializeField]
    Vector3 targetPosition;

    public event OnPathDelegate OnPathFound;
    public event OnPathDelegate OnPathTraversalCompleted;
   



    protected override void MovementUpdate(float deltaTime)
    {
        if (TargetReached)
            return;

        base.MovementUpdate(deltaTime);
    }


    /** Requests a path to the target */
    public override void SearchPath()
    {
        switch (m_Type)
        {
            case TargetType.Transform:
                base.SearchPath();
                break;
            case TargetType.Position:
                SearchPath(targetPosition);
                break;
        }

    }

    public void SearchPath(Vector3 pos)
    {
        SearchPath(pos, null);
    }
    public void SearchPath(Vector3 pos, OnPathDelegate callback)
    {
        lastRepath = Time.time;

        canSearchAgain = false;

        // Alternative way of requesting the path
        //ABPath p = ABPath.Construct(GetFeetPosition(), targetPosition, null);
        //seeker.StartPath(p);

        // We should search from the current position
        seeker.StartPath(GetFeetPosition(), pos, callback);
    }

    public void StopPathTraversal()
    {
        TargetReached = true;
    }


    public void SetTarget(Transform t)
    {
        m_Type = TargetType.Transform;
        target = t;
    }
    public void SetTarget(Vector3 pos)
    {
        m_Type = TargetType.Position;
        targetPosition = pos;
    }

    public void SetAndSearch(Transform t)
    {
        SetAndSearch(t, null);
    }
    public void SetAndSearch(Transform t, OnPathDelegate callback)
    {
        SetTarget(t);
        SearchPath();
    }
    public void SetAndSearch(Vector3 pos)
    {
        SetAndSearch(pos, null);
    }
    public void SetAndSearch(Vector3 pos, OnPathDelegate callback)
    {
        SetTarget(pos);
        SearchPath(pos, callback);
    }



    #region Accessors

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }
    public float Speed
    {
        get { return speed; }
    }
    public Vector3 Velocity
    {
        get { return velocity; }
    }
    public Path Path
    {
        get { return path;}
    }
    #endregion

}
