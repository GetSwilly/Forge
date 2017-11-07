using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAIPath : AIPath, IPathfinder, IMovement
{
    enum TargetType
    {
        Transform,
        Position
    }


    [SerializeField]
    bool isActive = true;

    [SerializeField]
    TargetType m_Type;

    [SerializeField]
    Vector3 targetPosition;

    float originalSpeed;
    //[SerializeField]
    //public bool shouldRotateTowardsPath = true;
    Dictionary<object, float> speedMultiplierSet = new Dictionary<object, float>();
    float speedMultiplier = 1f;

    Dictionary<object, float> rotationMultiplierSet = new Dictionary<object, float>();
    float rotationMultiplier = 1f;

    Transform m_Transform;
    CharacterController m_Character;
    Rigidbody m_Rigidbody;

    public event OnPathDelegate OnPathFound;
    public event OnPathDelegate OnPathTraversalCompleted;

    protected override void Awake()
    {
        base.Awake();

        m_Transform = GetComponent<Transform>();
        m_Character = GetComponent<CharacterController>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        originalSpeed = speed;
    }


    protected override void Update()
    {
        if (IsActive)
        {
            base.Update();
        }
    }
    protected override void FixedUpdate()
    {
        if (IsActive)
        {
            base.FixedUpdate();
        }
    }


    //protected override void Update()
    //{
    //    base.Update();

    //    if (TargetReached && OnPathTraversalCompleted != null)
    //    {
    //        OnPathTraversalCompleted(Path);
    //    }
    //}


    #region Movement

    public void Move(Vector3 direction)
    {
        Vector3 position = m_Transform.position;

        if (m_Character != null && m_Character.enabled)
        {
            m_Character.Move(direction * Speed * Time.deltaTime);
            position = m_Transform.position;
        }

        if (m_Rigidbody != null)
        {
            m_Rigidbody.MovePosition(position);
        }
        else
        {
            m_Transform.position = position;
        }
    }
    public void Move(Vector3 direction, float speed)
    {
        throw new NotImplementedException();
    }
    public void RotateTowards(Vector3 position)
    {
        Vector3 direction = position - tr.position;
        direction.y = 0;

        Quaternion desiredQ = Quaternion.LookRotation(direction);

        tr.rotation = Quaternion.RotateTowards(tr.rotation, desiredQ, rotationSpeed * Time.deltaTime);
    }

    public void AddSpeedMultiplier(object obj, float multiplier)
    {
        if (Mathf.Approximately(multiplier, 1f))
            return;

        if (speedMultiplierSet.ContainsKey(obj))
        {
            speedMultiplierSet[obj] = multiplier;
        }
        else
        {
            speedMultiplierSet.Add(obj, multiplier);
        }

        GetTotalSpeedMultiplier();
    }
    public void RemoveSpeedMultiplier(object obj)
    {
        if (!speedMultiplierSet.ContainsKey(obj))
        {
            return;
        }

        speedMultiplierSet.Remove(obj);

        GetTotalSpeedMultiplier();
    }
    void GetTotalSpeedMultiplier()
    {
        Dictionary<object, float>.Enumerator enumerator = speedMultiplierSet.GetEnumerator();

        float multiplier = 1f;

        while (enumerator.MoveNext())
        {
            multiplier *= enumerator.Current.Value;
        }

        SpeedMultiplier = multiplier;
    }



    public void AddRotationMultiplier(object obj, float multiplier)
    {
        if (Mathf.Approximately(multiplier, 1f))
            return;

        if (rotationMultiplierSet.ContainsKey(obj))
        {
            rotationMultiplierSet[obj] = multiplier;
        }
        else
        {
            rotationMultiplierSet.Add(obj, multiplier);
        }

        rotationMultiplier = GetTotalRotationMultiplier();
    }
    public void RemoveRotationMultiplier(object obj)
    {
        if (!rotationMultiplierSet.ContainsKey(obj))
        {
            return;
        }

        rotationMultiplierSet.Remove(obj);

        rotationMultiplier = GetTotalRotationMultiplier();
    }
    float GetTotalRotationMultiplier()
    {
        Dictionary<object, float>.Enumerator enumerator = rotationMultiplierSet.GetEnumerator();

        float multiplier = 1f;

        while (enumerator.MoveNext())
        {
            multiplier *= enumerator.Current.Value;
        }

        return multiplier;
    }

    #endregion


    #region Pathfinder

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
        CanMove = true;

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
        //CanMove = false;
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

    public float GetDistanceRemaining()
    {
        return interpolator.remainingDistance;
    }

    #endregion

    #region Accessors

    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }
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
    protected float SpeedMultiplier
    {
        get { return speedMultiplier; }
        set
        {
            speedMultiplier = value;
            speed = originalSpeed * speedMultiplier;
        }
    }
    public Vector3 Velocity
    {
        get { return velocity; }
    }
    public Path Path
    {
        get { return path; }
    }

    //public float EndReachedDistance
    //{
    //    get { return endReachedDistance; }
    //    set { endReachedDistance = Mathf.Clamp(value,0f,value); }
    //}
    //public float SlowdownDistance
    //{
    //    get { return slowdownDistance; }
    //    set { slowdownDistance = Mathf.Clamp(value,0f,value); }
    //}

    #endregion

    void OnValidate()
    {
        Speed = Speed;
    }
}
