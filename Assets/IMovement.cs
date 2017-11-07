using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovement {

    void Move(Vector3 direction);
    //void Move(Vector3 direction, float speed);
    void RotateTowards(Vector3 position);

    void AddSpeedMultiplier(object obj, float multiplier);
    void RemoveSpeedMultiplier(object obj);

    void AddRotationMultiplier(object obj, float multiplier);
    void RemoveRotationMultiplier(object obj);
}
