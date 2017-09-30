using UnityEngine;
using System;
using System.Collections;

public interface IProjectile {


    void Initialize(Transform newOwner, Team _team, Vector3 dir, int newPower, bool _critical, float newRange);
    void Initialize(Transform newOwner, Team _team, Vector3 dir, int newPower, bool _critical, float newSpeed, float newRange);
    
    void Disable();
    void Disable(Transform hitTransform);
    void Disable(Transform hitTransform, Vector3 hitPosition);




    Vector3 Position { get; }
    Vector3 Direction { get; }

    bool IsCritical { get; }
    Team Team { get; }
    Transform Owner { get; }
    int Power { get; }
    double MaxRange { get; set; }

    GameObject GameObject { get;}
    void SubscribeToOnImpact(Action<Health> alertMethod);
}
