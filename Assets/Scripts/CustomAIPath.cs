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

    //[SerializeField]
    //public bool shouldRotateTowardsPath = true;

    public event OnPathDelegate OnPathFound;
    public event OnPathDelegate OnPathTraversalCompleted;



    //protected override void Update()
    //{
    //    base.Update();

    //    if (TargetReached && OnPathTraversalCompleted != null)
    //    {
    //        OnPathTraversalCompleted(Path);
    //    }
    //}
    protected override void MovementUpdate(float deltaTime)
    {
        if (TargetReached)
            return;

        base.MovementUpdate(deltaTime);
    }
    //protected override void RotateTowards(Vector2 direction, float maxDegrees)
    //{
    //    if (!shouldRotateTowardsPath)
    //        return;

    //    base.RotateTowards(direction, maxDegrees);
    //}

    /** Requests a path to the target */
    public override void SearchPath()
    {
        switch (m_Type)
        {
            case TargetType.Transform:
                //base.SearchPath();
                SearchPathToTarget();
                break;
            case TargetType.Position:
                SearchPath(targetPosition);
                break;
        }

    }
    Vector3 GetClosestPosition(Transform t)
    {
        Collider coll = t.GetComponent<Collider>();

        if (coll == null)
            return t.position;

        Vector3 point = coll.ClosestPoint(transform.position);

        //Debug.DrawLine(transform.position, point, Color.cyan, 10f);

        return point;
    }

    void SearchPathToTarget()
    {
        if (target == null) throw new System.InvalidOperationException("Target is null");

        lastRepath = Time.time;
        // This is where we should search to
        Vector3 targetPosition = GetClosestPosition(target);

        canSearchAgain = false;
        
        // We should search from the current position
        seeker.StartPath(GetFeetPosition(), targetPosition);
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
        target = null;
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


    public override void OnTargetReached()
    {
        base.OnTargetReached();

        if (OnPathTraversalCompleted != null)
        {
            OnPathTraversalCompleted(Path);
        }
    }

    /** Called when a requested path has finished calculation.
	 * A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
	 * Finally it is returned to the seeker which forwards it to this function.\n
	 */
    public override void OnPathComplete(Path _p)
    {
        base.OnPathComplete(_p);

        if (OnPathFound != null)
        {
            OnPathFound(Path);
        }
    }

    public void RotateTowards(Vector3 position)
    {
        Vector3 direction = position - tr.position;
        direction.y = 0;

        // RotateTowards(direction, rotationSpeed * Time.deltaTime);

        Quaternion desiredQ = Quaternion.LookRotation(direction);

        tr.rotation = Quaternion.RotateTowards(tr.rotation, desiredQ, rotationSpeed * Time.deltaTime);
    }

    public float GetDistanceRemaining()
    {
        return interpolator.remainingDistance;
    }

    #region Accessors

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }
    //public bool ShouldRotateTowardsPath
    //{
    //    get { return shouldRotateTowardsPath; }
    //    set { shouldRotateTowardsPath = value; }
    //}
    public float Speed
    {
        get { return speed; }
        private set { speed = Mathf.Clamp(value, 0f, value); }
    }
    public Vector3 Velocity
    {
        get { return velocity; }
    }
    public Path Path
    {
        get { return path; }
    }
    #endregion

    void OnValidate()
    {
        Speed = Speed;
    }
}
