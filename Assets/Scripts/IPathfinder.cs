using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathfinder {

    event OnPathDelegate OnPathFound;
    event OnPathDelegate OnPathTraversalCompleted;

    void SearchPath();
    // void SearchPath(Transform t, OnPathDelegate callback);
    void SearchPath(Vector3 pos, OnPathDelegate callback);

    void SetTarget(Transform t);
    void SetTarget(Vector3 pos);

    void SetAndSearch(Transform t);
    void SetAndSearch(Transform t, OnPathDelegate callback);
    void SetAndSearch(Vector3 pos);
    void SetAndSearch(Vector3 pos, OnPathDelegate callback);

    void StopPathTraversal();

    bool CanMove { get; set; }
    Path Path { get; }
    float Speed { get; }
    Vector3 Velocity { get; }
}
